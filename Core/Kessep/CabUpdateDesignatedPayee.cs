// Program: CAB_UPDATE_DESIGNATED_PAYEE, ID: 371752456, model: 746.
// Short name: SWE00099
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_UPDATE_DESIGNATED_PAYEE.
/// </summary>
[Serializable]
public partial class CabUpdateDesignatedPayee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_UPDATE_DESIGNATED_PAYEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabUpdateDesignatedPayee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabUpdateDesignatedPayee.
  /// </summary>
  public CabUpdateDesignatedPayee(IContext context, Import import, Export export)
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
    // ***************************************************************
    // PR#82489 SWSRKXD 1/4/2000
    // - Delete views of ob_trn_desig_payee.
    // ***************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (!ReadCsePerson1())
    {
      ExitState = "FN0000_DESIG_PAYEE_CSE_PERSON_NF";

      return;
    }

    if (!ReadCsePerson2())
    {
      ExitState = "FN0000_PAYEE_CSE_PERSON_NF";

      return;
    }

    // << Validate Date Overlap with any other existing record >>
    // ****************************************************************
    // The check below only worked if the on-screen DP was already in the 
    // database associated to this Payee. A DP with overlapping dates that hadn'
    // t been this Payee's DP before was allowed.
    // Also changed the first where condition to be less than or equal to, from 
    // just less than. RK 9/21/98
    // ****************************************************************
    if (ReadCsePersonDesigPayee2())
    {
      ExitState = "FN0000_DATE_OVERLAP_WITH_DP";

      return;
    }
    else
    {
      // << No Date overlap...Continue >>
    }

    // << Read the cse-person-desig-payee to Update >>
    if (ReadCsePersonDesigPayee1())
    {
      try
      {
        UpdateCsePersonDesigPayee();
        import.BothTypeOfDpIn1View.LastUpdatedBy =
          entities.CsePersonDesigPayee.LastUpdatedBy;
        import.BothTypeOfDpIn1View.LastUpdatedTmst =
          entities.CsePersonDesigPayee.LastUpdatedTmst;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DESIG_PAYEE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DESIG_PAYEE_PV";

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
      ExitState = "FN0000_PERS_DESIG_PAYEE_NF_RB";
    }
  }

  private bool ReadCsePerson1()
  {
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.DesignatedPayee.Number);
      },
      (db, reader) =>
      {
        entities.DesignatedPayee.Number = db.GetString(reader, 0);
        entities.DesignatedPayee.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Payee.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Payee.Number);
      },
      (db, reader) =>
      {
        entities.Payee.Number = db.GetString(reader, 0);
        entities.Payee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee1()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee1",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", entities.Payee.Number);
        db.SetNullableString(
          command, "csePersNum", entities.DesignatedPayee.Number);
        db.SetInt32(
          command, "sequentialId",
          import.BothTypeOfDpIn1View.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadCsePersonDesigPayee2()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee2",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequentialId",
          import.BothTypeOfDpIn1View.SequentialIdentifier);
        db.SetString(command, "csePersoNum", entities.Payee.Number);
        db.SetDate(
          command, "effectiveDate1",
          import.BothTypeOfDpIn1View.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.BothTypeOfDpIn1View.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private void UpdateCsePersonDesigPayee()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonDesigPayee.Populated);

    var effectiveDate = import.BothTypeOfDpIn1View.EffectiveDate;
    var discontinueDate = import.BothTypeOfDpIn1View.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var notes = import.BothTypeOfDpIn1View.Notes ?? "";

    entities.CsePersonDesigPayee.Populated = false;
    Update("UpdateCsePersonDesigPayee",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "notes", notes);
        db.SetInt32(
          command, "sequentialId",
          entities.CsePersonDesigPayee.SequentialIdentifier);
        db.SetString(
          command, "csePersoNum", entities.CsePersonDesigPayee.CsePersoNum);
      });

    entities.CsePersonDesigPayee.EffectiveDate = effectiveDate;
    entities.CsePersonDesigPayee.DiscontinueDate = discontinueDate;
    entities.CsePersonDesigPayee.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonDesigPayee.LastUpdatedTmst = lastUpdatedTmst;
    entities.CsePersonDesigPayee.Notes = notes;
    entities.CsePersonDesigPayee.Populated = true;
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
    /// A value of DataPassedThruFlow.
    /// </summary>
    [JsonPropertyName("dataPassedThruFlow")]
    public Common DataPassedThruFlow
    {
      get => dataPassedThruFlow ??= new();
      set => dataPassedThruFlow = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
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
    /// A value of BothTypeOfDpIn1View.
    /// </summary>
    [JsonPropertyName("bothTypeOfDpIn1View")]
    public CsePersonDesigPayee BothTypeOfDpIn1View
    {
      get => bothTypeOfDpIn1View ??= new();
      set => bothTypeOfDpIn1View = value;
    }

    private Common dataPassedThruFlow;
    private CsePerson payee;
    private CsePerson designatedPayee;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private CsePersonDesigPayee bothTypeOfDpIn1View;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of BothTypeOfDpIn1View.
    /// </summary>
    [JsonPropertyName("bothTypeOfDpIn1View")]
    public CsePersonDesigPayee BothTypeOfDpIn1View
    {
      get => bothTypeOfDpIn1View ??= new();
      set => bothTypeOfDpIn1View = value;
    }

    private CsePersonDesigPayee bothTypeOfDpIn1View;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
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
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

    private CsePerson payee;
    private CsePerson designatedPayee;
    private ObligationTransaction obligationTransaction;
    private CsePersonDesigPayee csePersonDesigPayee;
  }
#endregion
}
