// Program: LE_B565_CRED_FIX_CONVERTED_DATA, ID: 372892884, model: 746.
// Short name: SWEL565B
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
/// A program: LE_B565_CRED_FIX_CONVERTED_DATA.
/// </para>
/// <para>
/// Program fixes problems with converted data which could not be corrected by 
/// the conversion team before implementation occured.
/// This job should be run after each conversion.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB565CredFixConvertedData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B565_CRED_FIX_CONVERTED_DATA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB565CredFixConvertedData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB565CredFixConvertedData.
  /// </summary>
  public LeB565CredFixConvertedData(IContext context, Import import,
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
    // --------------------------------------------------------
    // 09/03/99	PMcElderry
    // Original code
    // 09/09/99	PMcElderry
    // Type #6 fix
    // 09/21/99	PMcElderry
    // Type #7 fix
    // 10/03/00	PMcElderry
    // PR # 103819 - Errors made on stayed members.
    // ---------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Null1.DateSentToCra = null;

    if (Equal(global.Command, "CONVERSN"))
    {
      // -------------------
      // Fix converted data
      // -------------------
      foreach(var item in ReadCsePersonObligorCreditReporting2())
      {
        if (Equal(entities.CreditReporting.NotificationDate, null))
        {
          // ---------------------------------
          // Type 1 UPDATE - notification date
          // ---------------------------------
          try
          {
            UpdateCreditReporting6();
            ExitState = "ACO_NN0000_ALL_OK";
            ++local.TotalType1Changes.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        if (Equal(entities.CreditReporting.CurrentAmountDate, null))
        {
          if (ReadCreditReportingAction4())
          {
            // -----------------------------------
            // Type 2 UPDATE - current amount date
            // -----------------------------------
            try
            {
              UpdateCreditReporting2();
              ExitState = "ACO_NN0000_ALL_OK";
              ++local.TotalType2Changes.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }
        }

        if (Equal(entities.CreditReporting.DateSent, null) && entities
          .CreditReporting.NotificationDate != null)
        {
          if (ReadCreditReportingAction3())
          {
            // -------------------------
            // Type 5 UPDATE - date sent
            // -------------------------
            try
            {
              UpdateCreditReporting3();
              ExitState = "ACO_NN0000_ALL_OK";
              ++local.TotalType5Changes.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        if (ReadCreditReportingAction2())
        {
          if (!Equal(entities.CreditReportingAction.CurrentAmount,
            entities.CreditReportingAction.HighestAmount) || !
            Equal(entities.CreditReportingAction.OriginalAmount,
            entities.CreditReportingAction.CurrentAmount) || !
            Equal(entities.CreditReportingAction.HighestAmount,
            entities.CreditReportingAction.OriginalAmount))
          {
            // ---------------------------------------------------------------
            // At time of original coding, unsure of which field would be
            // populated.  Preference was given to Current_Amount
            // because of prior data.
            // POST CONVERSION:
            // At this time 9/5, Current_Amount is the only value
            // populated.
            // ---------------------------------------------------------------
            if (Lt(0, entities.CreditReportingAction.CurrentAmount))
            {
              // ---------------------------
              // Type 4 UPDATE - CRA amounts
              // ---------------------------
              try
              {
                UpdateCreditReportingAction4();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType4Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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
              if (Lt(0, entities.CreditReportingAction.OriginalAmount))
              {
                // ---------------------------
                // Type 4 UPDATE - CRA amounts
                // ---------------------------
                try
                {
                  UpdateCreditReportingAction2();
                  ExitState = "ACO_NN0000_ALL_OK";
                  ++local.TotalType4Changes.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
              else if (Lt(0, entities.CreditReportingAction.HighestAmount))
              {
                // ---------------------------
                // Type 4 UPDATE - CRA amounts
                // ---------------------------
                try
                {
                  UpdateCreditReportingAction1();
                  ExitState = "ACO_NN0000_ALL_OK";
                  ++local.TotalType4Changes.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }
            }
          }

          if (Equal(entities.CreditReporting.OriginalAmount, 0))
          {
            if (ReadCreditReportingAction6())
            {
              local.SetForActUpdate.Flag = "Y";
            }

            if (!IsEmpty(local.SetForActUpdate.Flag))
            {
              // ----------------------------------
              // Type 3 UPDATE - CR original amount
              // ----------------------------------
              try
              {
                UpdateCreditReporting8();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType3Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              local.SetForActUpdate.Flag = "";
            }
            else
            {
              // -------------------------
              // Type 3 UPDATE - CR amount
              // -------------------------
              try
              {
                UpdateCreditReporting7();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType3Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          // --------------------------------------------------------------
          // Following is for conversion records brought over that are
          // not only incomplete but functionally incorrect - multiple ACT
          // / ISS when it was not warranted.
          // --------------------------------------------------------------
          foreach(var item1 in ReadCreditReportingAction8())
          {
            if (Lt(0, entities.CreditReportingAction.CurrentAmount))
            {
              // ---------------
              // Type 4 A UPDATE
              // ---------------
              try
              {
                UpdateCreditReportingAction3();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType4AChanges.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            else if (Lt(0, entities.CreditReportingAction.OriginalAmount))
            {
              // ---------------
              // Type 4 A UPDATE
              // ---------------
              try
              {
                UpdateCreditReportingAction2();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType4AChanges.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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
              // -------------
              // Type 4 UPDATE
              // -------------
              try
              {
                UpdateCreditReportingAction1();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType4Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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

        if (Equal(entities.CreditReporting.HighestAmount, 0) || Equal
          (entities.CreditReporting.CurrentAmount, 0) || Equal
          (entities.CreditReporting.OriginalAmount, 0))
        {
          // ---------------------------
          // Type 6 UPDATE - CRA amounts
          // ---------------------------
          if (ReadCreditReportingAction4())
          {
            if (Equal(entities.CreditReportingAction.CseActionCode, "XBR") || Equal
              (entities.CreditReportingAction.CseActionCode, "XAD") || Equal
              (entities.CreditReportingAction.CseActionCode, "XGC"))
            {
            }
            else
            {
              try
              {
                UpdateCreditReporting1();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType6Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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

        if (entities.CreditReporting.DateStayed != null && Equal
          (entities.CreditReporting.DateStayReleased, null))
        {
          // ------------------------------------
          // Type 7 UPDATE - date stayed released
          // ------------------------------------
          if (ReadCreditReportingAction1())
          {
            if (Equal(entities.CreditReportingAction.CseActionCode, "CAN") || Equal
              (entities.CreditReportingAction.CseActionCode, "XAD"))
            {
              // ----------------------------------------------------------------
              // Because the action code is CAN, set it to the day of the run
              // date - stayed gets released at that point
              // ----------------------------------------------------------------
              try
              {
                UpdateCreditReporting4();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType7Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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
              // ----------------------------------------------------------------
              // Because the exact date of the release is not known, set it to
              // the day before the run date of the last applicable CRA run
              // ----------------------------------------------------------------
              try
              {
                UpdateCreditReporting5();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType7Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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

        // -----------------------------------------------------------
        // One record was found to have no ISS; just an ACT.  At
        // current (9/4/99) the following code is for that one record:
        // CSE # - 0000448745
        // -----------------------------------------------------------
        if (entities.CreditReportingAction.Identifier == 0)
        {
          foreach(var item1 in ReadCreditReportingAction9())
          {
            if (Equal(entities.CreditReporting.NotificationDate, null))
            {
              // ---------------------------------
              // Type 1 UPDATE - notification date
              // ---------------------------------
              try
              {
                UpdateCreditReporting6();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType1Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto ReadEach1;
              }
            }

            if (Equal(entities.CreditReporting.CurrentAmountDate, null))
            {
              if (ReadCreditReportingAction4())
              {
                // -----------------------------------
                // Type 2 UPDATE - current amount date
                // -----------------------------------
                try
                {
                  UpdateCreditReporting2();
                  ExitState = "ACO_NN0000_ALL_OK";
                  ++local.TotalType2Changes.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  goto ReadEach1;
                }
              }
            }

            if (Equal(entities.CreditReporting.DateSent, null) && entities
              .CreditReporting.NotificationDate != null)
            {
              if (ReadCreditReportingAction3())
              {
                // -------------------------
                // Type 5 UPDATE - date sent
                // -------------------------
                try
                {
                  UpdateCreditReporting3();
                  ExitState = "ACO_NN0000_ALL_OK";
                  ++local.TotalType5Changes.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(entities.CreditReporting.OriginalAmount, 0))
            {
              if (!Equal(entities.ActType.CurrentAmount,
                entities.ActType.HighestAmount) || !
                Equal(entities.ActType.OriginalAmount,
                entities.ActType.CurrentAmount) || !
                Equal(entities.ActType.HighestAmount,
                entities.ActType.OriginalAmount))
              {
                if (Lt(0, entities.ActType.CurrentAmount))
                {
                  // ---------------------------
                  // Type 4 UPDATE - CRA amounts
                  // ---------------------------
                  try
                  {
                    UpdateCreditReportingAction7();
                    ExitState = "ACO_NN0000_ALL_OK";
                    ++local.TotalType4Changes.Count;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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
                  if (Lt(0, entities.ActType.OriginalAmount))
                  {
                    // ---------------------------
                    // Type 4 UPDATE - CRA amounts
                    // ---------------------------
                    try
                    {
                      UpdateCreditReportingAction6();
                      ExitState = "ACO_NN0000_ALL_OK";
                      ++local.TotalType4Changes.Count;
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  else if (Lt(0, entities.ActType.HighestAmount))
                  {
                    // ---------------------------
                    // Type 4 UPDATE - CRA amounts
                    // ---------------------------
                    try
                    {
                      UpdateCreditReportingAction5();
                      ExitState = "ACO_NN0000_ALL_OK";
                      ++local.TotalType4Changes.Count;
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    goto ReadEach1;
                  }
                }
              }

              // ----------------------------------
              // Type 3 UPDATE - CR original amount
              // ----------------------------------
              try
              {
                UpdateCreditReporting8();
                ExitState = "ACO_NN0000_ALL_OK";
                ++local.TotalType3Changes.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto ReadEach1;
              }
            }

            if (entities.CreditReporting.DateStayed != null && Equal
              (entities.CreditReporting.DateStayReleased, null))
            {
              // ------------------------------------
              // Type 7 UPDATE - date stayed released
              // ------------------------------------
              if (ReadCreditReportingAction1())
              {
                if (Equal(entities.CreditReportingAction.CseActionCode, "CAN") ||
                  Equal(entities.CreditReportingAction.CseActionCode, "XAD"))
                {
                  // ----------------------------------------------------------------
                  // Because the action code is CAN, set it to the day of the 
                  // run
                  // date - stayed gets released at that point
                  // ----------------------------------------------------------------
                  try
                  {
                    UpdateCreditReporting4();
                    ExitState = "ACO_NN0000_ALL_OK";
                    ++local.TotalType7Changes.Count;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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
                  // ----------------------------------------------------------------
                  // Because the exact date of the release is not known, set it 
                  // to
                  // the day before the run date
                  // ----------------------------------------------------------------
                  try
                  {
                    UpdateCreditReporting5();
                    ExitState = "ACO_NN0000_ALL_OK";
                    ++local.TotalType7Changes.Count;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

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
          }
        }
      }

ReadEach1:

      local.ReportEabReportSend.ProcessDate = Now().Date;
      local.ReportEabReportSend.ProgramName = global.UserId;
      local.ReportEabFileHandling.Action = "OPEN";
      UseCabControlReport2();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        local.ReportFilesOpened.Flag = "Y";
      }
      else
      {
        ExitState = "OE0000_ERROR_READING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.ProgramName = global.UserId;
      local.ReportEabFileHandling.Action = "WRITE";
      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 1 UPDATES:  " + NumberToString
        (local.TotalType1Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 2 UPDATES:  " + NumberToString
        (local.TotalType2Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 3 UPDATES:  " + NumberToString
        (local.TotalType3Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 4 UPDATES:  " + NumberToString
        (local.TotalType4Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail =
        "TOTAL NUMBER TYPE 4 A UPDATES:  " + NumberToString
        (local.TotalType4AChanges.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 5 UPDATES:  " + NumberToString
        (local.TotalType5Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 6 UPDATES:  " + NumberToString
        (local.TotalType6Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "TOTAL NUMBER TYPE 7 UPDATES:  " + NumberToString
        (local.TotalType7Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        UseCabControlReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        local.ReportEabReportSend.ProgramName = global.UserId;
        local.ReportEabFileHandling.Action = "WRITE";
        local.ReportEabReportSend.RptDetail = "ERROR OCCURED AT CSE PERSON " + entities
          .CsePerson.Number;
        UseCabControlReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      local.ReportEabFileHandling.Action = "CLOSE";
      UseCabControlReport2();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      // ---------------------------------------------------------
      // Create credit reporting action delete (D04) records
      // ---------------------------------------------------------
      local.Cra.CraTransDate = Now().Date;

      foreach(var item in ReadCsePersonObligorCreditReporting1())
      {
        if (ReadCreditReportingAction5())
        {
          local.Cra.Identifier = 1 + entities.CreditReportingAction.Identifier;
        }

        foreach(var item1 in ReadCreditReportingAction7())
        {
          if (Lt(local.Null1.DateSentToCra,
            entities.CreditReportingAction.DateSentToCra) && Lt
            (entities.CreditReporting.DateStayed,
            entities.CreditReportingAction.DateSentToCra))
          {
            try
            {
              CreateCreditReportingAction();
              local.TotalType1Changes.Count = 1 + local.TotalType1Changes.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
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

          goto ReadEach2;
        }

ReadEach2:
        ;
      }

      local.ReportEabReportSend.ProcessDate = Now().Date;
      local.ReportEabReportSend.ProgramName = global.UserId;
      local.ReportEabFileHandling.Action = "OPEN";
      UseCabControlReport2();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        local.ReportFilesOpened.Flag = "Y";
      }
      else
      {
        ExitState = "OE0000_ERROR_READING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.ProgramName = global.UserId;
      local.ReportEabFileHandling.Action = "WRITE";
      local.ReportEabReportSend.RptDetail =
        "TOTAL NUMBER STAY DELETES CREATED:  " + NumberToString
        (local.TotalType1Changes.Count, 15);
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabFileHandling.Action = "CLOSE";
      UseCabControlReport2();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        // -------------------
        // continue processing
        // -------------------
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void CreateCreditReportingAction()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var identifier = local.Cra.Identifier;
    var cseActionCode = "DEL";
    var craTransCode = "D04";
    var craTransDate = local.Cra.CraTransDate;
    var originalAmount = entities.CreditReporting.OriginalAmount;
    var currentAmount = entities.CreditReporting.CurrentAmount;
    var highestAmount = entities.CreditReporting.HighestAmount;
    var cpaType = entities.CreditReporting.CpaType;
    var cspNumber = entities.CreditReporting.CspNumber;
    var aacType = entities.CreditReporting.Type1;
    var aacTakenDate = entities.CreditReporting.TakenDate;
    var aacTanfCode = entities.CreditReporting.TanfCode;

    CheckValid<CreditReportingAction>("CpaType", cpaType);
    CheckValid<CreditReportingAction>("AacType", aacType);
    entities.CreditReportingAction.Populated = false;
    Update("CreateCreditReportingAction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "cseActionCode", cseActionCode);
        db.SetString(command, "craTransCode", craTransCode);
        db.SetNullableDate(command, "craTransDate", craTransDate);
        db.SetNullableDate(command, "dateSentToCra", null);
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "highestAmount", highestAmount);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "aacType", aacType);
        db.SetDate(command, "aacTakenDate", aacTakenDate);
        db.SetString(command, "aacTanfCode", aacTanfCode);
      });

    entities.CreditReportingAction.Identifier = identifier;
    entities.CreditReportingAction.CseActionCode = cseActionCode;
    entities.CreditReportingAction.CraTransCode = craTransCode;
    entities.CreditReportingAction.CraTransDate = craTransDate;
    entities.CreditReportingAction.DateSentToCra = null;
    entities.CreditReportingAction.OriginalAmount = originalAmount;
    entities.CreditReportingAction.CurrentAmount = currentAmount;
    entities.CreditReportingAction.HighestAmount = highestAmount;
    entities.CreditReportingAction.CpaType = cpaType;
    entities.CreditReportingAction.CspNumber = cspNumber;
    entities.CreditReportingAction.AacType = aacType;
    entities.CreditReportingAction.AacTakenDate = aacTakenDate;
    entities.CreditReportingAction.AacTanfCode = aacTanfCode;
    entities.CreditReportingAction.Populated = true;
  }

  private bool ReadCreditReportingAction1()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction1",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetNullableDate(
          command, "craTransDate",
          entities.CreditReporting.DateStayed.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;
      });
  }

  private bool ReadCreditReportingAction2()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction2",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;
      });
  }

  private bool ReadCreditReportingAction3()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction3",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetNullableDate(
          command, "dateSentToCra",
          local.Null1.DateSentToCra.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;
      });
  }

  private bool ReadCreditReportingAction4()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction4",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;
      });
  }

  private bool ReadCreditReportingAction5()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return Read("ReadCreditReportingAction5",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;
      });
  }

  private bool ReadCreditReportingAction6()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.ActType.Populated = false;

    return Read("ReadCreditReportingAction6",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "craTransDate",
          entities.CreditReportingAction.CraTransDate.GetValueOrDefault());
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.ActType.Identifier = db.GetInt32(reader, 0);
        entities.ActType.CseActionCode = db.GetNullableString(reader, 1);
        entities.ActType.CraTransCode = db.GetString(reader, 2);
        entities.ActType.CraTransDate = db.GetNullableDate(reader, 3);
        entities.ActType.DateSentToCra = db.GetNullableDate(reader, 4);
        entities.ActType.OriginalAmount = db.GetNullableDecimal(reader, 5);
        entities.ActType.CurrentAmount = db.GetNullableDecimal(reader, 6);
        entities.ActType.HighestAmount = db.GetNullableDecimal(reader, 7);
        entities.ActType.CpaType = db.GetString(reader, 8);
        entities.ActType.CspNumber = db.GetString(reader, 9);
        entities.ActType.AacType = db.GetString(reader, 10);
        entities.ActType.AacTakenDate = db.GetDate(reader, 11);
        entities.ActType.AacTanfCode = db.GetString(reader, 12);
        entities.ActType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction7()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return ReadEach("ReadCreditReportingAction7",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction8()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.CreditReportingAction.Populated = false;

    return ReadEach("ReadCreditReportingAction8",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 2);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 8);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 9);
        entities.CreditReportingAction.AacType = db.GetString(reader, 10);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 11);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 12);
        entities.CreditReportingAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction9()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    entities.ActType.Populated = false;

    return ReadEach("ReadCreditReportingAction9",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
      },
      (db, reader) =>
      {
        entities.ActType.Identifier = db.GetInt32(reader, 0);
        entities.ActType.CseActionCode = db.GetNullableString(reader, 1);
        entities.ActType.CraTransCode = db.GetString(reader, 2);
        entities.ActType.CraTransDate = db.GetNullableDate(reader, 3);
        entities.ActType.DateSentToCra = db.GetNullableDate(reader, 4);
        entities.ActType.OriginalAmount = db.GetNullableDecimal(reader, 5);
        entities.ActType.CurrentAmount = db.GetNullableDecimal(reader, 6);
        entities.ActType.HighestAmount = db.GetNullableDecimal(reader, 7);
        entities.ActType.CpaType = db.GetString(reader, 8);
        entities.ActType.CspNumber = db.GetString(reader, 9);
        entities.ActType.AacType = db.GetString(reader, 10);
        entities.ActType.AacTakenDate = db.GetDate(reader, 11);
        entities.ActType.AacTanfCode = db.GetString(reader, 12);
        entities.ActType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligorCreditReporting1()
  {
    entities.Obligor.Populated = false;
    entities.CreditReporting.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonObligorCreditReporting1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateStayed", local.Null1.DateSentToCra.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CreditReporting.CspNumber = db.GetString(reader, 0);
        entities.CreditReporting.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.CreditReporting.CpaType = db.GetString(reader, 1);
        entities.CreditReporting.Type1 = db.GetString(reader, 2);
        entities.CreditReporting.TakenDate = db.GetDate(reader, 3);
        entities.CreditReporting.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.CreditReporting.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReporting.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.CreditReporting.NotificationDate =
          db.GetNullableDate(reader, 7);
        entities.CreditReporting.CreatedBy = db.GetString(reader, 8);
        entities.CreditReporting.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.CreditReporting.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.CreditReporting.DateSent = db.GetNullableDate(reader, 11);
        entities.CreditReporting.DateStayed = db.GetNullableDate(reader, 12);
        entities.CreditReporting.DateStayReleased =
          db.GetNullableDate(reader, 13);
        entities.CreditReporting.HighestAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CreditReporting.TanfCode = db.GetString(reader, 15);
        entities.Obligor.Populated = true;
        entities.CreditReporting.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligorCreditReporting2()
  {
    entities.Obligor.Populated = false;
    entities.CreditReporting.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonObligorCreditReporting2",
      null,
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CreditReporting.CspNumber = db.GetString(reader, 0);
        entities.CreditReporting.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.CreditReporting.CpaType = db.GetString(reader, 1);
        entities.CreditReporting.Type1 = db.GetString(reader, 2);
        entities.CreditReporting.TakenDate = db.GetDate(reader, 3);
        entities.CreditReporting.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.CreditReporting.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReporting.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.CreditReporting.NotificationDate =
          db.GetNullableDate(reader, 7);
        entities.CreditReporting.CreatedBy = db.GetString(reader, 8);
        entities.CreditReporting.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.CreditReporting.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.CreditReporting.DateSent = db.GetNullableDate(reader, 11);
        entities.CreditReporting.DateStayed = db.GetNullableDate(reader, 12);
        entities.CreditReporting.DateStayReleased =
          db.GetNullableDate(reader, 13);
        entities.CreditReporting.HighestAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CreditReporting.TanfCode = db.GetString(reader, 15);
        entities.Obligor.Populated = true;
        entities.CreditReporting.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private void UpdateCreditReporting1()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var originalAmount = entities.CreditReportingAction.OriginalAmount;
    var currentAmount = entities.CreditReportingAction.CurrentAmount;
    var lastUpdatedBy = "CONVCRED";
    var lastUpdatedTstamp = Now();
    var highestAmount = entities.CreditReportingAction.HighestAmount;

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting1",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDecimal(command, "highestAmount", highestAmount);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.OriginalAmount = originalAmount;
    entities.CreditReporting.CurrentAmount = currentAmount;
    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.HighestAmount = highestAmount;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting2()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var currentAmountDate = entities.CreditReportingAction.CraTransDate;
    var lastUpdatedBy = "CREDCONV";
    var lastUpdatedTstamp = Now();

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting2",
      (db, command) =>
      {
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.CurrentAmountDate = currentAmountDate;
    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting3()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var lastUpdatedBy = "CONVCRED";
    var lastUpdatedTstamp = Now();
    var dateSent = entities.CreditReportingAction.DateSentToCra;

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting3",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.DateSent = dateSent;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting4()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var lastUpdatedBy = "CONVCRED";
    var lastUpdatedTstamp = Now();
    var dateStayReleased = entities.CreditReportingAction.CraTransDate;

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting4",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "dateStayReleased", dateStayReleased);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.DateStayReleased = dateStayReleased;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting5()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var lastUpdatedBy = "CONVCRED";
    var lastUpdatedTstamp = Now();
    var dateStayReleased =
      AddDays(entities.CreditReportingAction.CraTransDate, -1);

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting5",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "dateStayReleased", dateStayReleased);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.DateStayReleased = dateStayReleased;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting6()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var notificationDate = entities.CreditReporting.TakenDate;
    var lastUpdatedBy = "CREDCONV";
    var lastUpdatedTstamp = Now();

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting6",
      (db, command) =>
      {
        db.SetNullableDate(command, "notificationDt", notificationDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting7()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var originalAmount = entities.CreditReportingAction.OriginalAmount;
    var lastUpdatedBy = "CREDCONV";
    var lastUpdatedTstamp = Now();

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting7",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.OriginalAmount = originalAmount;
    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReporting8()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var originalAmount = entities.ActType.OriginalAmount;
    var lastUpdatedBy = "CREDCONV";
    var lastUpdatedTstamp = Now();

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting8",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.OriginalAmount = originalAmount;
    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReportingAction1()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReportingAction.Populated);

    var originalAmount = entities.CreditReportingAction.HighestAmount;

    entities.CreditReportingAction.Populated = false;
    Update("UpdateCreditReportingAction1",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", originalAmount);
        db.SetInt32(
          command, "identifier", entities.CreditReportingAction.Identifier);
        db.
          SetString(command, "cpaType", entities.CreditReportingAction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.CreditReportingAction.CspNumber);
        db.
          SetString(command, "aacType", entities.CreditReportingAction.AacType);
          
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReportingAction.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode", entities.CreditReportingAction.AacTanfCode);
      });

    entities.CreditReportingAction.Populated = true;
  }

  private void UpdateCreditReportingAction2()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReportingAction.Populated);

    var currentAmount = entities.CreditReportingAction.OriginalAmount;

    entities.CreditReportingAction.Populated = false;
    Update("UpdateCreditReportingAction2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "highestAmount", currentAmount);
        db.SetInt32(
          command, "identifier", entities.CreditReportingAction.Identifier);
        db.
          SetString(command, "cpaType", entities.CreditReportingAction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.CreditReportingAction.CspNumber);
        db.
          SetString(command, "aacType", entities.CreditReportingAction.AacType);
          
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReportingAction.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode", entities.CreditReportingAction.AacTanfCode);
      });

    entities.CreditReportingAction.Populated = true;
  }

  private void UpdateCreditReportingAction3()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReportingAction.Populated);

    var originalAmount = entities.CreditReportingAction.CurrentAmount;

    entities.CreditReportingAction.Populated = false;
    Update("UpdateCreditReportingAction3",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
        db.SetInt32(
          command, "identifier", entities.CreditReportingAction.Identifier);
        db.
          SetString(command, "cpaType", entities.CreditReportingAction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.CreditReportingAction.CspNumber);
        db.
          SetString(command, "aacType", entities.CreditReportingAction.AacType);
          
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReportingAction.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode", entities.CreditReportingAction.AacTanfCode);
      });

    entities.CreditReportingAction.Populated = true;
  }

  private void UpdateCreditReportingAction4()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReportingAction.Populated);

    var originalAmount = entities.CreditReportingAction.CurrentAmount;

    entities.CreditReportingAction.Populated = false;
    Update("UpdateCreditReportingAction4",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
        db.SetInt32(
          command, "identifier", entities.CreditReportingAction.Identifier);
        db.
          SetString(command, "cpaType", entities.CreditReportingAction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.CreditReportingAction.CspNumber);
        db.
          SetString(command, "aacType", entities.CreditReportingAction.AacType);
          
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReportingAction.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode", entities.CreditReportingAction.AacTanfCode);
      });

    entities.CreditReportingAction.Populated = true;
  }

  private void UpdateCreditReportingAction5()
  {
    System.Diagnostics.Debug.Assert(entities.ActType.Populated);

    var originalAmount = entities.ActType.HighestAmount;

    entities.ActType.Populated = false;
    Update("UpdateCreditReportingAction5",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", originalAmount);
        db.SetInt32(command, "identifier", entities.ActType.Identifier);
        db.SetString(command, "cpaType", entities.ActType.CpaType);
        db.SetString(command, "cspNumber", entities.ActType.CspNumber);
        db.SetString(command, "aacType", entities.ActType.AacType);
        db.SetDate(
          command, "aacTakenDate",
          entities.ActType.AacTakenDate.GetValueOrDefault());
        db.SetString(command, "aacTanfCode", entities.ActType.AacTanfCode);
      });

    entities.ActType.Populated = true;
  }

  private void UpdateCreditReportingAction6()
  {
    System.Diagnostics.Debug.Assert(entities.ActType.Populated);

    var currentAmount = entities.ActType.OriginalAmount;

    entities.ActType.Populated = false;
    Update("UpdateCreditReportingAction6",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "highestAmount", currentAmount);
        db.SetInt32(command, "identifier", entities.ActType.Identifier);
        db.SetString(command, "cpaType", entities.ActType.CpaType);
        db.SetString(command, "cspNumber", entities.ActType.CspNumber);
        db.SetString(command, "aacType", entities.ActType.AacType);
        db.SetDate(
          command, "aacTakenDate",
          entities.ActType.AacTakenDate.GetValueOrDefault());
        db.SetString(command, "aacTanfCode", entities.ActType.AacTanfCode);
      });

    entities.ActType.Populated = true;
  }

  private void UpdateCreditReportingAction7()
  {
    System.Diagnostics.Debug.Assert(entities.ActType.Populated);

    var originalAmount = entities.ActType.CurrentAmount;

    entities.ActType.Populated = false;
    Update("UpdateCreditReportingAction7",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
        db.SetInt32(command, "identifier", entities.ActType.Identifier);
        db.SetString(command, "cpaType", entities.ActType.CpaType);
        db.SetString(command, "cspNumber", entities.ActType.CspNumber);
        db.SetString(command, "aacType", entities.ActType.AacType);
        db.SetDate(
          command, "aacTakenDate",
          entities.ActType.AacTakenDate.GetValueOrDefault());
        db.SetString(command, "aacTanfCode", entities.ActType.AacTanfCode);
      });

    entities.ActType.Populated = true;
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
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of Cra.
    /// </summary>
    [JsonPropertyName("cra")]
    public CreditReportingAction Cra
    {
      get => cra ??= new();
      set => cra = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CreditReportingAction Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of SetForActUpdate.
    /// </summary>
    [JsonPropertyName("setForActUpdate")]
    public Common SetForActUpdate
    {
      get => setForActUpdate ??= new();
      set => setForActUpdate = value;
    }

    /// <summary>
    /// A value of ReportFilesOpened.
    /// </summary>
    [JsonPropertyName("reportFilesOpened")]
    public Common ReportFilesOpened
    {
      get => reportFilesOpened ??= new();
      set => reportFilesOpened = value;
    }

    /// <summary>
    /// A value of ReportEabReportSend.
    /// </summary>
    [JsonPropertyName("reportEabReportSend")]
    public EabReportSend ReportEabReportSend
    {
      get => reportEabReportSend ??= new();
      set => reportEabReportSend = value;
    }

    /// <summary>
    /// A value of ReportEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportEabFileHandling")]
    public EabFileHandling ReportEabFileHandling
    {
      get => reportEabFileHandling ??= new();
      set => reportEabFileHandling = value;
    }

    /// <summary>
    /// A value of TotalType1Changes.
    /// </summary>
    [JsonPropertyName("totalType1Changes")]
    public Common TotalType1Changes
    {
      get => totalType1Changes ??= new();
      set => totalType1Changes = value;
    }

    /// <summary>
    /// A value of TotalType2Changes.
    /// </summary>
    [JsonPropertyName("totalType2Changes")]
    public Common TotalType2Changes
    {
      get => totalType2Changes ??= new();
      set => totalType2Changes = value;
    }

    /// <summary>
    /// A value of TotalType3Changes.
    /// </summary>
    [JsonPropertyName("totalType3Changes")]
    public Common TotalType3Changes
    {
      get => totalType3Changes ??= new();
      set => totalType3Changes = value;
    }

    /// <summary>
    /// A value of TotalType4Changes.
    /// </summary>
    [JsonPropertyName("totalType4Changes")]
    public Common TotalType4Changes
    {
      get => totalType4Changes ??= new();
      set => totalType4Changes = value;
    }

    /// <summary>
    /// A value of TotalType4AChanges.
    /// </summary>
    [JsonPropertyName("totalType4AChanges")]
    public Common TotalType4AChanges
    {
      get => totalType4AChanges ??= new();
      set => totalType4AChanges = value;
    }

    /// <summary>
    /// A value of TotalType5Changes.
    /// </summary>
    [JsonPropertyName("totalType5Changes")]
    public Common TotalType5Changes
    {
      get => totalType5Changes ??= new();
      set => totalType5Changes = value;
    }

    /// <summary>
    /// A value of TotalType6Changes.
    /// </summary>
    [JsonPropertyName("totalType6Changes")]
    public Common TotalType6Changes
    {
      get => totalType6Changes ??= new();
      set => totalType6Changes = value;
    }

    /// <summary>
    /// A value of TotalType7Changes.
    /// </summary>
    [JsonPropertyName("totalType7Changes")]
    public Common TotalType7Changes
    {
      get => totalType7Changes ??= new();
      set => totalType7Changes = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private AdministrativeActCertification creditReporting;
    private CreditReportingAction cra;
    private CreditReportingAction null1;
    private Common setForActUpdate;
    private Common reportFilesOpened;
    private EabReportSend reportEabReportSend;
    private EabFileHandling reportEabFileHandling;
    private Common totalType1Changes;
    private Common totalType2Changes;
    private Common totalType3Changes;
    private Common totalType4Changes;
    private Common totalType4AChanges;
    private Common totalType5Changes;
    private Common totalType6Changes;
    private Common totalType7Changes;
    private External passArea;
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
    /// A value of ActType.
    /// </summary>
    [JsonPropertyName("actType")]
    public CreditReportingAction ActType
    {
      get => actType ??= new();
      set => actType = value;
    }

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
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
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

    private CsePersonAccount obligor;
    private CreditReportingAction actType;
    private AdministrativeActCertification creditReporting;
    private CreditReportingAction creditReportingAction;
    private CsePerson csePerson;
  }
#endregion
}
