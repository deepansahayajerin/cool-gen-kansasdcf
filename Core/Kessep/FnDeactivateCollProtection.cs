// Program: FN_DEACTIVATE_COLL_PROTECTION, ID: 373387965, model: 746.
// Short name: SWE00493
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DEACTIVATE_COLL_PROTECTION.
/// </summary>
[Serializable]
public partial class FnDeactivateCollProtection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DEACTIVATE_COLL_PROTECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeactivateCollProtection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeactivateCollProtection.
  /// </summary>
  public FnDeactivateCollProtection(IContext context, Import import,
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
    if (ReadObligationCsePerson())
    {
      if (AsChar(entities.Obligation.PrimarySecondaryCode) != 'S')
      {
        if (ReadObligCollProtectionHist())
        {
          try
          {
            UpdateObligCollProtectionHist();

            // ----------------------------------------------------------------
            // moved from here
            // ----------------------------------------------------------------
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OB_COLL_PROT_HIST_NF_RB";

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
        else
        {
          ExitState = "FN0000_OB_COLL_PROT_HIST_NF_RB";

          return;
        }
      }

      // ----------------------------------------------------------------
      // to here
      // ----------------------------------------------------------------
      foreach(var item in ReadCollection())
      {
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

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ----------------------------------------------------------------
      // "Supported" is a subtype of cse person account.  "Debt" is a suptype of
      // obligation transaction.  When we Deactivate any timeframe, set the
      // trigger to p and the pgm effective date for all supported persons for
      // the obligation. Only update if the effective date is greater than the
      // import coll start date.
      // ----------------------------------------------------------------
      foreach(var item in ReadSupported())
      {
        if (entities.Supported.PgmChgEffectiveDate != null && Lt
          (entities.Supported.PgmChgEffectiveDate,
          import.ObligCollProtectionHist.CvrdCollStrtDt))
        {
          return;
        }

        try
        {
          UpdateSupported();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_SUPP_PRSN_ACCT_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_SUPP_PERSON_ACCT_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (Lt(import.ObligCollProtectionHist.CvrdCollStrtDt,
          entities.Supported.PgmChgEffectiveDate) && !
          Equal(entities.Supported.PgmChgEffectiveDate, null))
        {
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
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
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 14);
        entities.Collection.DistributionMethod = db.GetString(reader, 15);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
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
          import.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollEndDt",
          import.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
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
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ObligCollProtectionHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 6);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 7);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 8);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 9);
        entities.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadObligationCsePerson()
  {
    entities.Obligation.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadObligationCsePerson",
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
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadSupported()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Supported.Populated = false;

    return ReadEach("ReadSupported",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Supported.LastUpdatedTmst = db.GetNullableDateTime(reader, 3);
        entities.Supported.PgmChgEffectiveDate = db.GetNullableDate(reader, 4);
        entities.Supported.TriggerType = db.GetNullableString(reader, 5);
        entities.Supported.Populated = true;

        return true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var distributionMethod = "A";

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

  private void UpdateObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(entities.ObligCollProtectionHist.Populated);

    var deactivationDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ObligCollProtectionHist.Populated = false;
    Update("UpdateObligCollProtectionHist",
      (db, command) =>
      {
        db.SetDate(command, "deactivationDate", deactivationDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetDateTime(
          command, "createdTmst",
          entities.ObligCollProtectionHist.CreatedTmst.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.ObligCollProtectionHist.CspNumber);
        db.SetString(
          command, "cpaType", entities.ObligCollProtectionHist.CpaType);
        db.SetInt32(
          command, "otyIdentifier",
          entities.ObligCollProtectionHist.OtyIdentifier);
        db.SetInt32(
          command, "obgIdentifier",
          entities.ObligCollProtectionHist.ObgIdentifier);
      });

    entities.ObligCollProtectionHist.DeactivationDate = deactivationDate;
    entities.ObligCollProtectionHist.LastUpdatedBy = lastUpdatedBy;
    entities.ObligCollProtectionHist.LastUpdatedTmst = lastUpdatedTmst;
    entities.ObligCollProtectionHist.Populated = true;
  }

  private void UpdateSupported()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = import.ObligCollProtectionHist.CvrdCollStrtDt;
    var triggerType = "P";

    entities.Supported.Populated = false;
    Update("UpdateSupported",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", triggerType);
        db.SetString(command, "cspNumber", entities.Supported.CspNumber);
        db.SetString(command, "type", entities.Supported.Type1);
      });

    entities.Supported.LastUpdatedBy = lastUpdatedBy;
    entities.Supported.LastUpdatedTmst = lastUpdatedTmst;
    entities.Supported.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.Supported.TriggerType = triggerType;
    entities.Supported.Populated = true;
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

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private ObligCollProtectionHist obligCollProtectionHist;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ObligCollProtectionHist Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private ObligCollProtectionHist null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CsePersonAccount supported;
    private ObligationTransaction debt;
    private CsePersonAccount csePersonAccount;
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
