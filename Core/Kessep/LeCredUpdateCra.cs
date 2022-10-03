// Program: LE_CRED_UPDATE_CRA, ID: 372586589, model: 746.
// Short name: SWE00748
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CRED_UPDATE_CRA.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates a 'D04' record for the cancellation or deletion of
/// a credit reporting action. In the case of cancellation, it also clears
/// Notified date so that if the obligor meets the criteria at a later time, the
/// system will send the notice again. If action = SPACES it updates Date
/// Stayed, Date Stay Released &amp; AP Resp date
/// ---------------------------------------------
/// </para>
/// </summary>
[Serializable]
public partial class LeCredUpdateCra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CRED_UPDATE_CRA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCredUpdateCra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCredUpdateCra.
  /// </summary>
  public LeCredUpdateCra(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 02/19/1996	govind				Initial Coding
    // 10/09/98     P. Sharp     Removed read of CSE person and made cse number 
    // part of the qualifier in the credit report read each. Removed
    // cse_action_code validation. This was done in the procedure step.
    // 11/17/99	R. Jean	PR79604 - Create credit reporting action rows for
    // stay date changes(STA), stay release date changes (RST),
    // response date date changes (RAP).
    // -------------------------------------------------------------------
    MoveAdministrativeActCertification(import.CreditReporting,
      export.CreditReporting);
    local.Current.Date = Now().Date;
    local.NoOfAdmActCertRecs.Count = 0;

    if (ReadCreditReporting())
    {
      ++local.NoOfAdmActCertRecs.Count;

      // *****************************************************************
      // * 11/17/99	R. Jean	PR79604 -Capture prior image of adm act cert
      // *****************************************************************
      local.OriginalAdmActCert.Assign(entities.ExistingAdmActCert);

      if (IsEmpty(import.UserAction.CseActionCode))
      {
        foreach(var item in ReadCreditReportingAction3())
        {
          if (Equal(entities.Last.CseActionCode, "CAN"))
          {
            // --- It is okay to update date stayed and date stay released 
            // fields since the last cra action was cancelled.
            break;
          }
          else
          {
            break;
          }
        }

        // --- either no CRA record was found OR the last sent record was a CAN 
        // record.
        try
        {
          UpdateCreditReporting1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CREDIT_REPORTING_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (Equal(import.UserAction.CseActionCode, "DEL"))
      {
        try
        {
          UpdateCreditReporting2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CREDIT_REPORTING_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ---------------------------------------------
      // For both CAN and DEL
      // ---------------------------------------------
      if (ReadCreditReportingAction1())
      {
        // ---------------------------------------------
        // If the latest record was a D04 record, then dont allow creation of 
        // another one if the current cse action is DEL. However if the current
        // cse action is CAN, it needs to be processed since we need to cancel
        // the notice.
        // Last cse action   Current cse action  Allow
        //       CAN              CAN             No
        //       DEL              DEL             No
        //       CAN              DEL             No
        //       DEL              CAN            Yes
        // ---------------------------------------------
        switch(TrimEnd(entities.Last.CseActionCode))
        {
          case "CAN":
            if (Equal(import.UserAction.CseActionCode, "CAN") || Equal
              (import.UserAction.CseActionCode, "DEL"))
            {
              ExitState = "LE0000_CRED_ALREADY_CANCELED";

              return;
            }

            break;
          case "DEL":
            if (Equal(import.UserAction.CseActionCode, "DEL"))
            {
              ExitState = "LE0000_CRED_DEL_ALREADY_CREATED";

              return;
            }

            break;
          default:
            break;
        }

        local.Last.Identifier = entities.Last.Identifier;
      }

      // *****************************************************************
      // * 12/9/99	R. Jean	PR79604 - Because of a conversion problem
      // where the identifier is in the inverse order compared
      // with the CRA trans date, read in descending ID order
      // *****************************************************************
      if (ReadCreditReportingAction2())
      {
        if (entities.LastId.Identifier > local.Last.Identifier)
        {
          local.Last.Identifier = entities.LastId.Identifier;
        }
      }

      if (Equal(import.UserAction.CseActionCode, "CAN") || Equal
        (import.UserAction.CseActionCode, "DEL"))
      {
        local.CreditReportingAction.CseActionCode =
          import.UserAction.CseActionCode ?? "";
        local.CreditReportingAction.CraTransCode = "DDA";

        // *****************************************************************
        // * 11/17/99	R. Jean	PR79604 - Read latest ACT, UPD, or ISS transaction
        // *****************************************************************
        foreach(var item in ReadCreditReportingAction4())
        {
          switch(TrimEnd(entities.Last.CseActionCode))
          {
            case "ISS":
              break;
            case "ACT":
              break;
            case "UPD":
              break;
            case "SND":
              break;
            default:
              continue;
          }

          break;
        }

        // *****************************************************************
        // * 11/17/99	R. Jean	PR79604 - If the date sent to CRA is date(0)
        // * then it has not been sent to CRA.  Thus do not set the
        // * CRA trans code to D04 (delete).
        // *****************************************************************
        if (Equal(import.UserAction.CseActionCode, "CAN") && Equal
          (entities.Last.DateSentToCra, local.InitialisedToZeros.Date))
        {
          local.CreditReportingAction.CraTransCode = "";
        }

        if (Equal(import.UserAction.CseActionCode, "DEL") && Equal
          (entities.Last.DateSentToCra, local.InitialisedToZeros.Date))
        {
          local.CreditReportingAction.CraTransCode = "";
        }

        try
        {
          CreateCreditReportingAction1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CREDIT_REPORTING_ACTION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
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
        // *****************************************************************
        // * 11/17/99	R. Jean	PR79604 - Determine what is being updated and 
        // create appropriate credit report action row.  Multiple rows can be
        // created!
        // *****************************************************************
        if (!Equal(import.CreditReporting.DateStayed,
          local.OriginalAdmActCert.DateStayed) && !
          Equal(import.CreditReporting.DateStayed, local.InitialisedToZeros.Date))
          
        {
          local.CreditReportingAction.CseActionCode = "STA";

          try
          {
            CreateCreditReportingAction2();
            ++local.Last.Identifier;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CREDIT_REPORTING_ACTION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (!Equal(import.CreditReporting.DateStayReleased,
          local.OriginalAdmActCert.DateStayReleased) && !
          Equal(import.CreditReporting.DateStayReleased,
          local.InitialisedToZeros.Date))
        {
          local.CreditReportingAction.CseActionCode = "RST";

          try
          {
            CreateCreditReportingAction2();
            ++local.Last.Identifier;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CREDIT_REPORTING_ACTION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (!Equal(import.CreditReporting.ApRespReceivedData,
          local.OriginalAdmActCert.ApRespReceivedData) && !
          Equal(import.CreditReporting.ApRespReceivedData,
          local.InitialisedToZeros.Date))
        {
          local.CreditReportingAction.CseActionCode = "RAP";

          try
          {
            CreateCreditReportingAction2();
            ++local.Last.Identifier;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CREDIT_REPORTING_ACTION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      // ---------------------------------------------
      // After processing the latest administrative act certification, escape 
      // out. There must only be one administrative act certification record for
      // an obligor; but there may be more than one credit_reporting_action
      // record.
      // ---------------------------------------------
    }

    if (local.NoOfAdmActCertRecs.Count == 0)
    {
      ExitState = "LE0000_NO_CRED_ADMIN_ACTION_TAKN";
    }
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.ApRespReceivedData = source.ApRespReceivedData;
    target.DateStayed = source.DateStayed;
    target.DateStayReleased = source.DateStayReleased;
  }

  private void CreateCreditReportingAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);

    var identifier = local.Last.Identifier + 1;
    var cseActionCode = local.CreditReportingAction.CseActionCode ?? "";
    var craTransCode = local.CreditReportingAction.CraTransCode;
    var craTransDate = local.Current.Date;
    var dateSentToCra = local.InitialisedToZeros.Date;
    var originalAmount = 0M;
    var cpaType = entities.ExistingAdmActCert.CpaType;
    var cspNumber = entities.ExistingAdmActCert.CspNumber;
    var aacType = entities.ExistingAdmActCert.Type1;
    var aacTakenDate = entities.ExistingAdmActCert.TakenDate;
    var aacTanfCode = entities.ExistingAdmActCert.TanfCode;

    CheckValid<CreditReportingAction>("CpaType", cpaType);
    CheckValid<CreditReportingAction>("AacType", aacType);
    entities.New1.Populated = false;
    Update("CreateCreditReportingAction1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "cseActionCode", cseActionCode);
        db.SetString(command, "craTransCode", craTransCode);
        db.SetNullableDate(command, "craTransDate", craTransDate);
        db.SetNullableDate(command, "dateSentToCra", dateSentToCra);
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", originalAmount);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "aacType", aacType);
        db.SetDate(command, "aacTakenDate", aacTakenDate);
        db.SetString(command, "aacTanfCode", aacTanfCode);
      });

    entities.New1.Identifier = identifier;
    entities.New1.CseActionCode = cseActionCode;
    entities.New1.CraTransCode = craTransCode;
    entities.New1.CraTransDate = craTransDate;
    entities.New1.DateSentToCra = dateSentToCra;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = originalAmount;
    entities.New1.HighestAmount = originalAmount;
    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.AacType = aacType;
    entities.New1.AacTakenDate = aacTakenDate;
    entities.New1.AacTanfCode = aacTanfCode;
    entities.New1.Populated = true;
  }

  private void CreateCreditReportingAction2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);

    var identifier = local.Last.Identifier + 1;
    var cseActionCode = local.CreditReportingAction.CseActionCode ?? "";
    var craTransDate = local.Current.Date;
    var dateSentToCra = local.InitialisedToZeros.Date;
    var originalAmount = 0M;
    var cpaType = entities.ExistingAdmActCert.CpaType;
    var cspNumber = entities.ExistingAdmActCert.CspNumber;
    var aacType = entities.ExistingAdmActCert.Type1;
    var aacTakenDate = entities.ExistingAdmActCert.TakenDate;
    var aacTanfCode = entities.ExistingAdmActCert.TanfCode;

    CheckValid<CreditReportingAction>("CpaType", cpaType);
    CheckValid<CreditReportingAction>("AacType", aacType);
    entities.New1.Populated = false;
    Update("CreateCreditReportingAction2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "cseActionCode", cseActionCode);
        db.SetString(command, "craTransCode", "");
        db.SetNullableDate(command, "craTransDate", craTransDate);
        db.SetNullableDate(command, "dateSentToCra", dateSentToCra);
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", originalAmount);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "aacType", aacType);
        db.SetDate(command, "aacTakenDate", aacTakenDate);
        db.SetString(command, "aacTanfCode", aacTanfCode);
      });

    entities.New1.Identifier = identifier;
    entities.New1.CseActionCode = cseActionCode;
    entities.New1.CraTransCode = "";
    entities.New1.CraTransDate = craTransDate;
    entities.New1.DateSentToCra = dateSentToCra;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = originalAmount;
    entities.New1.HighestAmount = originalAmount;
    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.AacType = aacType;
    entities.New1.AacTakenDate = aacTakenDate;
    entities.New1.AacTanfCode = aacTanfCode;
    entities.New1.Populated = true;
  }

  private bool ReadCreditReporting()
  {
    entities.ExistingAdmActCert.Populated = false;

    return Read("ReadCreditReporting",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAdmActCert.CpaType = db.GetString(reader, 0);
        entities.ExistingAdmActCert.CspNumber = db.GetString(reader, 1);
        entities.ExistingAdmActCert.Type1 = db.GetString(reader, 2);
        entities.ExistingAdmActCert.TakenDate = db.GetDate(reader, 3);
        entities.ExistingAdmActCert.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingAdmActCert.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingAdmActCert.DecertifiedDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingAdmActCert.NotificationDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingAdmActCert.NotifiedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingAdmActCert.ApRespReceivedData =
          db.GetNullableDate(reader, 9);
        entities.ExistingAdmActCert.DateStayed = db.GetNullableDate(reader, 10);
        entities.ExistingAdmActCert.DateStayReleased =
          db.GetNullableDate(reader, 11);
        entities.ExistingAdmActCert.HighestAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingAdmActCert.TanfCode = db.GetString(reader, 13);
        entities.ExistingAdmActCert.DecertificationReason =
          db.GetNullableString(reader, 14);
        entities.ExistingAdmActCert.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingAdmActCert.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingAdmActCert.Type1);
      });
  }

  private bool ReadCreditReportingAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);
    entities.Last.Populated = false;

    return Read("ReadCreditReportingAction1",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.ExistingAdmActCert.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.ExistingAdmActCert.Type1);
        db.SetString(
          command, "aacTanfCode", entities.ExistingAdmActCert.TanfCode);
        db.
          SetString(command, "cspNumber", entities.ExistingAdmActCert.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingAdmActCert.CpaType);
      },
      (db, reader) =>
      {
        entities.Last.Identifier = db.GetInt32(reader, 0);
        entities.Last.CseActionCode = db.GetNullableString(reader, 1);
        entities.Last.CraTransCode = db.GetString(reader, 2);
        entities.Last.CraTransDate = db.GetNullableDate(reader, 3);
        entities.Last.DateSentToCra = db.GetNullableDate(reader, 4);
        entities.Last.CpaType = db.GetString(reader, 5);
        entities.Last.CspNumber = db.GetString(reader, 6);
        entities.Last.AacType = db.GetString(reader, 7);
        entities.Last.AacTakenDate = db.GetDate(reader, 8);
        entities.Last.AacTanfCode = db.GetString(reader, 9);
        entities.Last.Populated = true;
        CheckValid<CreditReportingAction>("CpaType", entities.Last.CpaType);
        CheckValid<CreditReportingAction>("AacType", entities.Last.AacType);
      });
  }

  private bool ReadCreditReportingAction2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);
    entities.LastId.Populated = false;

    return Read("ReadCreditReportingAction2",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.ExistingAdmActCert.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.ExistingAdmActCert.Type1);
        db.SetString(
          command, "aacTanfCode", entities.ExistingAdmActCert.TanfCode);
        db.
          SetString(command, "cspNumber", entities.ExistingAdmActCert.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingAdmActCert.CpaType);
      },
      (db, reader) =>
      {
        entities.LastId.Identifier = db.GetInt32(reader, 0);
        entities.LastId.CseActionCode = db.GetNullableString(reader, 1);
        entities.LastId.CraTransCode = db.GetString(reader, 2);
        entities.LastId.CraTransDate = db.GetNullableDate(reader, 3);
        entities.LastId.DateSentToCra = db.GetNullableDate(reader, 4);
        entities.LastId.CpaType = db.GetString(reader, 5);
        entities.LastId.CspNumber = db.GetString(reader, 6);
        entities.LastId.AacType = db.GetString(reader, 7);
        entities.LastId.AacTakenDate = db.GetDate(reader, 8);
        entities.LastId.AacTanfCode = db.GetString(reader, 9);
        entities.LastId.Populated = true;
        CheckValid<CreditReportingAction>("CpaType", entities.LastId.CpaType);
        CheckValid<CreditReportingAction>("AacType", entities.LastId.AacType);
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);
    entities.Last.Populated = false;

    return ReadEach("ReadCreditReportingAction3",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.ExistingAdmActCert.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.ExistingAdmActCert.Type1);
        db.SetString(
          command, "aacTanfCode", entities.ExistingAdmActCert.TanfCode);
        db.
          SetString(command, "cspNumber", entities.ExistingAdmActCert.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingAdmActCert.CpaType);
        db.SetNullableDate(
          command, "dateSentToCra",
          local.InitialisedToZeros.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Last.Identifier = db.GetInt32(reader, 0);
        entities.Last.CseActionCode = db.GetNullableString(reader, 1);
        entities.Last.CraTransCode = db.GetString(reader, 2);
        entities.Last.CraTransDate = db.GetNullableDate(reader, 3);
        entities.Last.DateSentToCra = db.GetNullableDate(reader, 4);
        entities.Last.CpaType = db.GetString(reader, 5);
        entities.Last.CspNumber = db.GetString(reader, 6);
        entities.Last.AacType = db.GetString(reader, 7);
        entities.Last.AacTakenDate = db.GetDate(reader, 8);
        entities.Last.AacTanfCode = db.GetString(reader, 9);
        entities.Last.Populated = true;
        CheckValid<CreditReportingAction>("CpaType", entities.Last.CpaType);
        CheckValid<CreditReportingAction>("AacType", entities.Last.AacType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);
    entities.Last.Populated = false;

    return ReadEach("ReadCreditReportingAction4",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.ExistingAdmActCert.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.ExistingAdmActCert.Type1);
        db.SetString(
          command, "aacTanfCode", entities.ExistingAdmActCert.TanfCode);
        db.
          SetString(command, "cspNumber", entities.ExistingAdmActCert.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingAdmActCert.CpaType);
      },
      (db, reader) =>
      {
        entities.Last.Identifier = db.GetInt32(reader, 0);
        entities.Last.CseActionCode = db.GetNullableString(reader, 1);
        entities.Last.CraTransCode = db.GetString(reader, 2);
        entities.Last.CraTransDate = db.GetNullableDate(reader, 3);
        entities.Last.DateSentToCra = db.GetNullableDate(reader, 4);
        entities.Last.CpaType = db.GetString(reader, 5);
        entities.Last.CspNumber = db.GetString(reader, 6);
        entities.Last.AacType = db.GetString(reader, 7);
        entities.Last.AacTakenDate = db.GetDate(reader, 8);
        entities.Last.AacTanfCode = db.GetString(reader, 9);
        entities.Last.Populated = true;
        CheckValid<CreditReportingAction>("CpaType", entities.Last.CpaType);
        CheckValid<CreditReportingAction>("AacType", entities.Last.AacType);

        return true;
      });
  }

  private void UpdateCreditReporting1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);

    var apRespReceivedData = import.CreditReporting.ApRespReceivedData;
    var dateStayed = import.CreditReporting.DateStayed;
    var dateStayReleased = import.CreditReporting.DateStayReleased;

    entities.ExistingAdmActCert.Populated = false;
    Update("UpdateCreditReporting1",
      (db, command) =>
      {
        db.SetNullableDate(command, "apRespRecdDate", apRespReceivedData);
        db.SetNullableDate(command, "dateStayed", dateStayed);
        db.SetNullableDate(command, "dateStayReleased", dateStayReleased);
        db.SetString(command, "cpaType", entities.ExistingAdmActCert.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingAdmActCert.CspNumber);
          
        db.SetString(command, "type", entities.ExistingAdmActCert.Type1);
        db.SetDate(
          command, "takenDt",
          entities.ExistingAdmActCert.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.ExistingAdmActCert.TanfCode);
      });

    entities.ExistingAdmActCert.ApRespReceivedData = apRespReceivedData;
    entities.ExistingAdmActCert.DateStayed = dateStayed;
    entities.ExistingAdmActCert.DateStayReleased = dateStayReleased;
    entities.ExistingAdmActCert.Populated = true;
  }

  private void UpdateCreditReporting2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAdmActCert.Populated);

    var decertifiedDate = local.Current.Date;
    var notificationDate = local.InitialisedToZeros.Date;
    var decertificationReason = "DELETED";

    entities.ExistingAdmActCert.Populated = false;
    Update("UpdateCreditReporting2",
      (db, command) =>
      {
        db.SetNullableDate(command, "decertifiedDt", decertifiedDate);
        db.SetNullableDate(command, "notificationDt", notificationDate);
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableDate(command, "apRespRecdDate", notificationDate);
        db.SetNullableDate(command, "dateStayed", notificationDate);
        db.SetNullableDate(command, "dateStayReleased", notificationDate);
        db.SetNullableString(command, "decertifyReason", decertificationReason);
        db.SetString(command, "cpaType", entities.ExistingAdmActCert.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingAdmActCert.CspNumber);
          
        db.SetString(command, "type", entities.ExistingAdmActCert.Type1);
        db.SetDate(
          command, "takenDt",
          entities.ExistingAdmActCert.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.ExistingAdmActCert.TanfCode);
      });

    entities.ExistingAdmActCert.DecertifiedDate = decertifiedDate;
    entities.ExistingAdmActCert.NotificationDate = notificationDate;
    entities.ExistingAdmActCert.NotifiedBy = "";
    entities.ExistingAdmActCert.ApRespReceivedData = notificationDate;
    entities.ExistingAdmActCert.DateStayed = notificationDate;
    entities.ExistingAdmActCert.DateStayReleased = notificationDate;
    entities.ExistingAdmActCert.DecertificationReason = decertificationReason;
    entities.ExistingAdmActCert.Populated = true;
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
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public CreditReportingAction UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private AdministrativeActCertification creditReporting;
    private CreditReportingAction userAction;
    private CsePersonsWorkSet obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    private AdministrativeActCertification creditReporting;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CreditReportingAction Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of NoOfAdmActCertRecs.
    /// </summary>
    [JsonPropertyName("noOfAdmActCertRecs")]
    public Common NoOfAdmActCertRecs
    {
      get => noOfAdmActCertRecs ??= new();
      set => noOfAdmActCertRecs = value;
    }

    /// <summary>
    /// A value of OriginalAdmActCert.
    /// </summary>
    [JsonPropertyName("originalAdmActCert")]
    public AdministrativeActCertification OriginalAdmActCert
    {
      get => originalAdmActCert ??= new();
      set => originalAdmActCert = value;
    }

    private CreditReportingAction creditReportingAction;
    private DateWorkArea current;
    private CreditReportingAction last;
    private DateWorkArea initialisedToZeros;
    private Common noOfAdmActCertRecs;
    private AdministrativeActCertification originalAdmActCert;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LastId.
    /// </summary>
    [JsonPropertyName("lastId")]
    public CreditReportingAction LastId
    {
      get => lastId ??= new();
      set => lastId = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CreditReportingAction Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CreditReportingAction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingAdmActCert.
    /// </summary>
    [JsonPropertyName("existingAdmActCert")]
    public AdministrativeActCertification ExistingAdmActCert
    {
      get => existingAdmActCert ??= new();
      set => existingAdmActCert = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CsePersonAccount Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CreditReportingAction lastId;
    private CreditReportingAction last;
    private CreditReportingAction new1;
    private CsePerson existingObligor;
    private AdministrativeActCertification existingAdmActCert;
    private CsePersonAccount existing;
  }
#endregion
}
