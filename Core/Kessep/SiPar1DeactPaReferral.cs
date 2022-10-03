// Program: SI_PAR1_DEACT_PA_REFERRAL, ID: 371759863, model: 746.
// Short name: SWE01151
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PAR1_DEACT_PA_REFERRAL.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiPar1DeactPaReferral: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PAR1_DEACT_PA_REFERRAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPar1DeactPaReferral(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPar1DeactPaReferral.
  /// </summary>
  public SiPar1DeactPaReferral(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    //      M A I N T E N A N C E   L O G
    //  Date     Developer     Description
    // 3-01-95   J.W. Hays     Initial development
    // 8-29-95   Ken Evans     Continue development
    // 10/02/98  W. Campbell   Modified the logic so that
    //                         ROLLBACKs will be performed when
    //                         Database updates (for both DB2 and
    //                         ADABAS) don't work properly.
    //                         Changes were made to:
    //                         SI_PAR1_PA_REFERRAL_PAGE_1
    //                         SI_PAR1_DEACT_PA_REFERRAL
    //                         SI_PAR1_ASSOC_PA_REFERRAL_CASE
    // ------------------------------------------------------------
    // 06/29/99 M.Lachowicz    Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // *********************************************
    // This CAB deactivates a PA referral by setting
    // the deactivate ind and deactivate date.
    // *********************************************
    local.Current.Date = Now().Date;

    // 06/29/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadPaReferral())
    {
      try
      {
        UpdatePaReferral();

        foreach(var item in ReadPaReferralAssignment())
        {
          try
          {
            UpdatePaReferralAssignment();
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PA_REFERRAL_ASSIGNMENT_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PA_REFERRAL_ASSIGNMENT_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PA_REFERRAL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PA_REFERRAL_PV";

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
      ExitState = "PA_REFERRAL_NF";
    }
  }

  private bool ReadPaReferral()
  {
    entities.ExistingPaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetString(command, "type", import.PaReferral.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 4);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingPaReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaReferralAssignment()
  {
    entities.ExistingPaReferralAssignment.Populated = false;

    return ReadEach("ReadPaReferralAssignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "pafType", entities.ExistingPaReferral.Type1);
        db.SetString(command, "pafNo", entities.ExistingPaReferral.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferralAssignment.EffectiveDate =
          db.GetDate(reader, 0);
        entities.ExistingPaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ExistingPaReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ExistingPaReferralAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ExistingPaReferralAssignment.OffId = db.GetInt32(reader, 6);
        entities.ExistingPaReferralAssignment.OspCode = db.GetString(reader, 7);
        entities.ExistingPaReferralAssignment.OspDate = db.GetDate(reader, 8);
        entities.ExistingPaReferralAssignment.PafNo = db.GetString(reader, 9);
        entities.ExistingPaReferralAssignment.PafType =
          db.GetString(reader, 10);
        entities.ExistingPaReferralAssignment.PafTstamp =
          db.GetDateTime(reader, 11);
        entities.ExistingPaReferralAssignment.Populated = true;

        return true;
      });
  }

  private void UpdatePaReferral()
  {
    var assignDeactivateInd = import.PaReferral.AssignDeactivateInd ?? "";
    var assignDeactivateDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingPaReferral.Populated = false;
    Update("UpdatePaReferral",
      (db, command) =>
      {
        db.SetNullableString(command, "assignDeactInd", assignDeactivateInd);
        db.SetNullableDate(command, "assignDeactDate", assignDeactivateDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "numb", entities.ExistingPaReferral.Number);
        db.SetString(command, "type", entities.ExistingPaReferral.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ExistingPaReferral.AssignDeactivateInd = assignDeactivateInd;
    entities.ExistingPaReferral.AssignDeactivateDate = assignDeactivateDate;
    entities.ExistingPaReferral.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingPaReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingPaReferral.Populated = true;
  }

  private void UpdatePaReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPaReferralAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingPaReferralAssignment.Populated = false;
    Update("UpdatePaReferralAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingPaReferralAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.ExistingPaReferralAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.ExistingPaReferralAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.ExistingPaReferralAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ExistingPaReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "pafNo", entities.ExistingPaReferralAssignment.PafNo);
        db.SetString(
          command, "pafType", entities.ExistingPaReferralAssignment.PafType);
        db.SetDateTime(
          command, "pafTstamp",
          entities.ExistingPaReferralAssignment.PafTstamp.GetValueOrDefault());
      });

    entities.ExistingPaReferralAssignment.DiscontinueDate = discontinueDate;
    entities.ExistingPaReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingPaReferralAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingPaReferralAssignment.Populated = true;
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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    private PaReferral paReferral;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPaReferralAssignment.
    /// </summary>
    [JsonPropertyName("existingPaReferralAssignment")]
    public PaReferralAssignment ExistingPaReferralAssignment
    {
      get => existingPaReferralAssignment ??= new();
      set => existingPaReferralAssignment = value;
    }

    /// <summary>
    /// A value of ExistingPaReferral.
    /// </summary>
    [JsonPropertyName("existingPaReferral")]
    public PaReferral ExistingPaReferral
    {
      get => existingPaReferral ??= new();
      set => existingPaReferral = value;
    }

    private PaReferralAssignment existingPaReferralAssignment;
    private PaReferral existingPaReferral;
  }
#endregion
}
