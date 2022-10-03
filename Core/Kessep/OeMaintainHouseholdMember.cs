// Program: OE_MAINTAIN_HOUSEHOLD_MEMBER, ID: 374449059, model: 746.
// Short name: SWE02875
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_MAINTAIN_HOUSEHOLD_MEMBER.
/// </summary>
[Serializable]
public partial class OeMaintainHouseholdMember: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MAINTAIN_HOUSEHOLD_MEMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMaintainHouseholdMember(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMaintainHouseholdMember.
  /// </summary>
  public OeMaintainHouseholdMember(IContext context, Import import,
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
    // ******** MAINTENANCE LOG 
    // ***************************************
    // AUTHOR          DATE  	  CHG REQ# DESCRIPTION
    // Ed Lyman    06/01/2000	  Initial Code
    // Ed Lyman    08/08/2000    Medical Grant Allowed for AF Program
    // Ed Lyman    08/14/2000    Temporary fix to disable program check
    // Paul Phinney 03/02/2001   I00114795
    // Allow "NF" and "NC" for BOTH Regular URA and Medical
    // Added "FC" and "SI" to Medical per Kit Cole
    // ******** END MAINTENANCE LOG 
    // ***********************************
    // *** Import summary date is always the first of the month.    ***
    // *** When comparing to Person Program, the program can        ***
    // *** can be effective any day of the month and still qualify. ***
    // *** Therefore make the summary date the last day of the      ***
    // *** month when comparing to program effective date.          ***
    if (ReadImHousehold())
    {
      if (ReadCsePerson())
      {
        if (!Lt(import.Summary.Date, import.Boundary.Date))
        {
          local.LastDayOfMonth.Date =
            AddDays(AddMonths(import.Summary.Date, 1), -1);

          if (import.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() > 0
            )
          {
            // *** Grants added for a date after the boundary date must be 
            // foster care. ***
            // Paul Phinney 03/02/2001   I00114795
            // Allow "NF" and "NC" for BOTH Regular URA and Medical
            if (!ReadPersonProgram1())
            {
              ExitState = "OE0000_FC_PGM_REQUIRED_FOR_GRANT";

              return;
            }
          }
          else
          {
            // Paul Phinney 03/02/2001   I00114795
            // Allow "NF" and "NC" for BOTH Regular URA and Medical
            // Added "FC" and "SI" to Medical per Kit Cole
            if (!ReadPersonProgram2())
            {
              ExitState = "OE0000_MED_PGM_REQUIRE_FOR_GRANT";

              return;
            }
          }
        }

        if (ReadImHouseholdMbrMnthlySum())
        {
          MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum,
            local.ImHouseholdMbrMnthlySum);

          if (import.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() > 0
            )
          {
            // * It is ok to add a regular grant to an existing summary that has
            //   only a medical grant.  Make sure that no regular grant or
            //   URA adjustment has been made.  ***
            if (local.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() ==
              0 && local
              .ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() == 0)
            {
              local.ImHouseholdMbrMnthlySum.GrantAmount =
                import.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
              local.ImHouseholdMbrMnthlySum.UraAmount =
                import.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
            }
            else
            {
              ExitState = "OE0000_IM_HH_MBR_MONTHLY_SUM_AE";

              return;
            }
          }
          else
          {
            // * It is ok to add a medical grant to an existing summary that has
            //   only a regular grant.  Make sure that no medical grant or
            //   URA medical adjustment has been made.  ***
            if (local.ImHouseholdMbrMnthlySum.GrantMedicalAmount.
              GetValueOrDefault() == 0 && local
              .ImHouseholdMbrMnthlySum.UraMedicalAmount.GetValueOrDefault() == 0
              )
            {
              local.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
                import.ImHouseholdMbrMnthlySum.GrantMedicalAmount.
                  GetValueOrDefault();
              local.ImHouseholdMbrMnthlySum.UraMedicalAmount =
                import.ImHouseholdMbrMnthlySum.GrantMedicalAmount.
                  GetValueOrDefault();
            }
            else
            {
              ExitState = "OE0000_IM_HH_MBR_MONTHLY_SUM_AE";

              return;
            }
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
                ExitState = "OE0000_IM_HH_MBR_MONTHLY_SUM_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OE0000_IM_HH_MBR_MONTHLY_SUM_PV";

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
            CreateImHouseholdMbrMnthlySum();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0000_IM_HH_MBR_MONTHLY_SUM_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OE0000_IM_HH_MBR_MONTHLY_SUM_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        foreach(var item in ReadImHouseholdMbrMnthlySumCsePersonAccount())
        {
          if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
            local.LowValue.Date) || Lt
            (import.Summary.Date, entities.CsePersonAccount.PgmChgEffectiveDate))
            
          {
            try
            {
              UpdateCsePersonAccount();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }
    else
    {
      ExitState = "OE0000_IM_HOUSEHOLD_NF";
    }
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.GrantAmount = source.GrantAmount;
    target.GrantMedicalAmount = source.GrantMedicalAmount;
    target.UraAmount = source.UraAmount;
    target.UraMedicalAmount = source.UraMedicalAmount;
  }

  private void CreateImHouseholdMbrMnthlySum()
  {
    var year = import.ImHouseholdMbrMnthlySum.Year;
    var month = import.ImHouseholdMbrMnthlySum.Month;
    var relationship = import.ImHouseholdMbrMnthlySum.Relationship;
    var grantAmount =
      import.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
    var grantMedicalAmount =
      import.ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTmst = Now();
    var lastUpdatedTmst = local.LowValue.Timestamp;
    var imhAeCaseNo = entities.ImHousehold.AeCaseNo;
    var cspNumber = entities.CsePerson.Number;

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("CreateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "year0", year);
        db.SetInt32(command, "month0", month);
        db.SetString(command, "relationship", relationship);
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "grantMedAmt", grantMedicalAmount);
        db.SetNullableDecimal(command, "uraAmount", grantAmount);
        db.SetNullableDecimal(command, "uraMedicalAmount", grantMedicalAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.Year = year;
    entities.ImHouseholdMbrMnthlySum.Month = month;
    entities.ImHouseholdMbrMnthlySum.Relationship = relationship;
    entities.ImHouseholdMbrMnthlySum.GrantAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount = grantMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.UraAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.UraMedicalAmount = grantMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlySum.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = "";
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlySum.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "month0", import.ImHouseholdMbrMnthlySum.Month);
        db.SetInt32(command, "year0", import.ImHouseholdMbrMnthlySum.Year);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ImHouseholdMbrMnthlySum.CreatedBy = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 11);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 12);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySumCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;
    entities.ImHousehold.Populated = false;
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySumCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetInt32(command, "month0", import.ImHouseholdMbrMnthlySum.Month);
        db.SetInt32(command, "year0", import.ImHouseholdMbrMnthlySum.Year);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ImHouseholdMbrMnthlySum.CreatedBy = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 11);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 11);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 12);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 13);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 14);
        entities.CsePersonAccount.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CsePersonAccount.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 16);
        entities.CsePersonAccount.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 17);
        entities.CsePersonAccount.TriggerType =
          db.GetNullableString(reader, 18);
        entities.CsePersonAccount.Populated = true;
        entities.ImHousehold.Populated = true;
        entities.ImHouseholdMbrMnthlySum.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.LastDayOfMonth.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Summary.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 7);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 8);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 9);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 11);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.LastDayOfMonth.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Summary.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 7);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 8);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 9);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 11);
        entities.PersonProgram.Populated = true;
      });
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = import.Summary.Date;
    var triggerType = "U";

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", triggerType);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(command, "type", entities.CsePersonAccount.Type1);
      });

    entities.CsePersonAccount.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAccount.LastUpdatedTmst = lastUpdatedTmst;
    entities.CsePersonAccount.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.CsePersonAccount.TriggerType = triggerType;
    entities.CsePersonAccount.Populated = true;
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var grantAmount =
      local.ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
    var grantMedicalAmount =
      local.ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault();
    var uraAmount = local.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
    var uraMedicalAmount =
      local.ImHouseholdMbrMnthlySum.UraMedicalAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "grantAmt", grantAmount);
        db.SetNullableDecimal(command, "grantMedAmt", grantMedicalAmount);
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
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

    entities.ImHouseholdMbrMnthlySum.GrantAmount = grantAmount;
    entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount = grantMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.UraMedicalAmount = uraMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
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
    /// A value of Boundary.
    /// </summary>
    [JsonPropertyName("boundary")]
    public DateWorkArea Boundary
    {
      get => boundary ??= new();
      set => boundary = value;
    }

    /// <summary>
    /// A value of Summary.
    /// </summary>
    [JsonPropertyName("summary")]
    public DateWorkArea Summary
    {
      get => summary ??= new();
      set => summary = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private DateWorkArea boundary;
    private DateWorkArea summary;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private CsePerson csePerson;
    private ImHousehold imHousehold;
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
    /// A value of LastDayOfMonth.
    /// </summary>
    [JsonPropertyName("lastDayOfMonth")]
    public DateWorkArea LastDayOfMonth
    {
      get => lastDayOfMonth ??= new();
      set => lastDayOfMonth = value;
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
    /// A value of LowValue.
    /// </summary>
    [JsonPropertyName("lowValue")]
    public DateWorkArea LowValue
    {
      get => lowValue ??= new();
      set => lowValue = value;
    }

    private DateWorkArea lastDayOfMonth;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private DateWorkArea lowValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private CsePersonAccount csePersonAccount;
    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }
#endregion
}
