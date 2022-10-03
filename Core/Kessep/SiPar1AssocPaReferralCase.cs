// Program: SI_PAR1_ASSOC_PA_REFERRAL_CASE, ID: 371759865, model: 746.
// Short name: SWE01551
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PAR1_ASSOC_PA_REFERRAL_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiPar1AssocPaReferralCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PAR1_ASSOC_PA_REFERRAL_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPar1AssocPaReferralCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPar1AssocPaReferralCase.
  /// </summary>
  public SiPar1AssocPaReferralCase(IContext context, Import import,
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
    // ---------------------------------------------
    //      M A I N T E N A N C E   L O G
    //  Date     Developer     Description
    // 6-27-96   Rao Mulpuri   Initial development
    // ---------------------------------------------
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
    // This CAB associates a PA referral to Case
    // after returning from REGI.
    // It also updates the AP PA_Referral_Participant
    // with the AP person # in the CSE Case.
    // *********************************************
    // 06/29/99 M.L         Change property of READ to generate
    //                      Select Only
    // ------------------------------------------------------------
    if (ReadPaReferral())
    {
      // 06/29/99 M.L         Change property of READ to generate
      //                      Select Only
      // ------------------------------------------------------------
      if (ReadCase())
      {
        try
        {
          UpdatePaReferral();

          if (!IsEmpty(import.Ap.Number))
          {
            // 06/29/99 M.L         Change property of READ to generate
            //                      Select Only
            // ------------------------------------------------------------
            if (ReadPaReferralParticipant())
            {
              if (IsEmpty(entities.Ap.PersonNumber))
              {
                try
                {
                  UpdatePaReferralParticipant();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "PA_REFERRAL_PARTICIPANT_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "PA_REFERRAL_PARTICIPANT_PV";

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
              ExitState = "PA_REFERRRAL_PARTICIPANT_NF";
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
        ExitState = "CASE_NF";
      }
    }
    else
    {
      ExitState = "PA_REFERRAL_NF";
    }
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.ExistingPaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "type", import.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 1);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingPaReferral.CasNumber = db.GetNullableString(reader, 5);
        entities.ExistingPaReferral.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant()
  {
    entities.Ap.Populated = false;

    return Read("ReadPaReferralParticipant",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.ExistingPaReferral.Number);
        db.SetString(command, "pafType", entities.ExistingPaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.Ap.Identifier = db.GetInt32(reader, 0);
        entities.Ap.PersonNumber = db.GetNullableString(reader, 1);
        entities.Ap.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Ap.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Ap.PreNumber = db.GetString(reader, 4);
        entities.Ap.PafType = db.GetString(reader, 5);
        entities.Ap.PafTstamp = db.GetDateTime(reader, 6);
        entities.Ap.Role = db.GetNullableString(reader, 7);
        entities.Ap.Populated = true;
      });
  }

  private void UpdatePaReferral()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var casNumber = entities.ExistingCase.Number;

    entities.ExistingPaReferral.Populated = false;
    Update("UpdatePaReferral",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetString(command, "numb", entities.ExistingPaReferral.Number);
        db.SetString(command, "type", entities.ExistingPaReferral.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ExistingPaReferral.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingPaReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingPaReferral.CasNumber = casNumber;
    entities.ExistingPaReferral.Populated = true;
  }

  private void UpdatePaReferralParticipant()
  {
    System.Diagnostics.Debug.Assert(entities.Ap.Populated);

    var personNumber = import.Ap.Number;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Ap.Populated = false;
    Update("UpdatePaReferralParticipant",
      (db, command) =>
      {
        db.SetNullableString(command, "personNum", personNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.Ap.Identifier);
        db.SetString(command, "preNumber", entities.Ap.PreNumber);
        db.SetString(command, "pafType", entities.Ap.PafType);
        db.SetDateTime(
          command, "pafTstamp", entities.Ap.PafTstamp.GetValueOrDefault());
      });

    entities.Ap.PersonNumber = personNumber;
    entities.Ap.LastUpdatedBy = lastUpdatedBy;
    entities.Ap.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Ap.Populated = true;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    private CsePersonsWorkSet ap;
    private Case1 case1;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public PaReferralParticipant Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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

    private PaReferralParticipant ap;
    private Case1 existingCase;
    private PaReferral existingPaReferral;
  }
#endregion
}
