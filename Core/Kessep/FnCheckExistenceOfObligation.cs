// Program: FN_CHECK_EXISTENCE_OF_OBLIGATION, ID: 372084596, model: 746.
// Short name: SWE01589
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CHECK_EXISTENCE_OF_OBLIGATION.
/// </summary>
[Serializable]
public partial class FnCheckExistenceOfObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_EXISTENCE_OF_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckExistenceOfObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckExistenceOfObligation.
  /// </summary>
  public FnCheckExistenceOfObligation(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // ==
    // ==	This action block is used only by OACC
    // ==
    // =================================================
    // =================================================================================
    // 08/05/2011  RMathews   CQ11647   Modified date edit when adding an 
    // obligation to
    //                                  
    // prevent multiple obligations for
    // same date
    // =================================================================================
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(import.EndDate.Date, local.EndDate.Date))
    {
      local.EndDate.Date = import.Maximum.Date;
    }
    else
    {
      local.EndDate.Date = import.EndDate.Date;
    }

    // ***  Deleted Read of legal_action.  Already exists; replaced with SOME / 
    // THAT selection criteria of other entity types. ***
    if (ReadLegalActionDetail())
    {
      if (Equal(entities.LegalActionDetail.CreatedBy, "CONVERSN") && IsEmpty
        (entities.LegalActionDetail.FreqPeriodCode))
      {
        ExitState = "FN0000_CONVRSN_OBLIG_UPDATE_LEGL";

        return;
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    if (import.ObligationType.SystemGeneratedIdentifier == 0)
    {
      if (ReadObligationType2())
      {
        export.ObligationType.Assign(entities.ObligationType);
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_TYPE_INVALID";

        return;
      }
    }
    else if (ReadObligationType1())
    {
      export.ObligationType.Assign(entities.ObligationType);
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_TYPE_INVALID";

      return;
    }

    if (export.Obligation.SystemGeneratedIdentifier == 0 || export
      .ObligationType.SystemGeneratedIdentifier == 0 || IsEmpty
      (export.Obligor.Number))
    {
      // **** This is the path when control will flow from LDET and obligation 
      // id is unknown ****
      local.PathInd.Flag = "1";
    }
    else
    {
      // **** This is the path when control will flow when obligation-id is 
      // known ****
      local.PathInd.Flag = "2";
    }

    if (AsChar(local.PathInd.Flag) == '2')
    {
      if (ReadCsePerson())
      {
        export.Obligor.Number = entities.ObligorCsePerson.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    export.Common.Flag = "N";

    switch(AsChar(local.PathInd.Flag))
    {
      case '1':
        // **** flowed from LDET
        // **** Find out if there is any currently active obligation ****
        foreach(var item in ReadCsePersonCsePerson())
        {
          // =================================================
          // 6/25/99 - b adams  -  Added SORTED BY end_dt to support
          //   the need to add timeframes for interstate obligations.
          // =================================================
          foreach(var item1 in ReadObligationObligationPaymentScheduleObligationTransaction2())
            
          {
            if (Equal(global.Command, "DELETE") || Equal
              (global.Command, "UPDATE"))
            {
              if (entities.Obligation.SystemGeneratedIdentifier != import
                .Obligation.SystemGeneratedIdentifier)
              {
                continue;
              }
            }

            // -----------------------------------------------------------------------
            // In case of Joint/several, we must store  the  AP/Payor number in
            // 'export_obligor cse_person' and pass that value to PSTEP. Make
            // sure that AP/Payor number is READ first.
            // The IF make sure that the AP/Payor number will not be overwritten
            // by joint/several  number.
            // ---------------------------------------------------------------------------
            if (IsEmpty(export.Obligor.Number))
            {
              export.Obligor.Number = entities.ObligorForImportLegDet.Number;
            }

            export.Obligation.SystemGeneratedIdentifier =
              entities.Obligation.SystemGeneratedIdentifier;

            // =================================================
            // 6/23/99 - bud adams  -  Existing debts that are being ended
            //   will be ignored so another time-frame can be added.
            // =================================================
            if (Equal(import.BeforeLink.Command, "TYPE") && Equal
              (global.Command, "DISPLAY"))
            {
              if (!Equal(entities.ObligationPaymentSchedule.EndDt,
                import.Maximum.Date))
              {
                // =================================================
                // And if any of those previous debts were interstate, make a
                // note of it.  If not, then the one being created MUST be.
                // =================================================
                if (Equal(entities.Obligation.OtherStateAbbr, "KS"))
                {
                  if (IsEmpty(export.ExistingInterstateDebt.Flag))
                  {
                    export.ExistingInterstateDebt.Flag = "N";
                  }
                }
                else
                {
                  export.ExistingInterstateDebt.Flag = "Y";
                }

                if (local.Common.Count == 1)
                {
                  // ***---  the most recent one is needed for display 
                  // information
                  export.Obligation.SystemGeneratedIdentifier =
                    local.Active.SystemGeneratedIdentifier;
                  export.Previous.EndDt =
                    entities.ObligationPaymentSchedule.EndDt;
                }
                else
                {
                  local.Active.SystemGeneratedIdentifier =
                    entities.Obligation.SystemGeneratedIdentifier;
                  local.Common.Count = 1;
                  export.Common.Flag = "Y";
                }

                // ***---  check if an earlier one exists that is interstate
                continue;
              }
              else
              {
                ExitState = "FN0000_ACTIVE_OBLIGATION_EXISTS";

                return;
              }
            }

            // **** If command = Display, Update or Delete we need to check if 
            // there is an obligation that is 'active' currently. If it is that
            // needs to be displayed, else the message no active obligation
            // should come and the user should be allowed to add a fresh
            // obligation
            // ****
            if (Equal(import.Common.Command, "UPDATE") || Equal
              (import.Common.Command, "DELETE") || Equal
              (import.Common.Command, "DISPLAY"))
            {
              // =================================================
              // 6/28/99 - bud adams  -  If this Legal_Action_Detail has many
              //   Obligations and they are not History Obligations, then this
              //   Detail is for an obligation that supports interstate time
              //   frames.  The Start Date for this cannot be prior to the End
              //   Date of the previous obligation.
              // =================================================
              if (local.Common.Count == 1)
              {
                export.Obligation.SystemGeneratedIdentifier =
                  local.Active.SystemGeneratedIdentifier;
                export.Previous.EndDt =
                  entities.ObligationPaymentSchedule.EndDt;

                return;
              }

              local.Active.SystemGeneratedIdentifier =
                entities.Obligation.SystemGeneratedIdentifier;
              export.Common.Flag = "Y";
              local.Common.Count = 1;

              continue;
            }

            // **** if command = ADD we need to check if the period for which 
            // the obligation is being added, no obligation exists ****
            if (Equal(import.Common.Command, "ADD") && IsEmpty
              (import.BeforeLink.Command))
            {
              // CQ11647  Check input end date >= existing start date
              if (!Lt(entities.ObligationPaymentSchedule.EndDt,
                import.StartDate.StartDt) && !
                Lt(local.EndDate.Date,
                entities.ObligationPaymentSchedule.StartDt))
              {
                export.Common.Flag = "Y";

                return;
              }
            }
          }
        }

        break;
      case '2':
        // **** Find out if there is any currently active obligation ****
        export.Common.Flag = "N";

        foreach(var item in ReadCsePersonCsePerson())
        {
          foreach(var item1 in ReadObligationObligationPaymentScheduleObligationTransaction1())
            
          {
            if (Equal(global.Command, "DELETE") || Equal
              (global.Command, "UPDATE"))
            {
              if (entities.Obligation.SystemGeneratedIdentifier != import
                .Obligation.SystemGeneratedIdentifier)
              {
                continue;
              }
            }

            export.Obligor.Number = entities.ObligorForImportLegDet.Number;
            export.Obligation.SystemGeneratedIdentifier =
              entities.Obligation.SystemGeneratedIdentifier;

            // **** If command = Display, Update or Delete we need to check if 
            // there is an obligation that is 'active' currently. If it is that
            // needs to be displayed, else the message no active obligation
            // should come and the user should be allowed to add a fresh
            // obligation
            // ****
            ++local.Common.Count;

            if (Equal(import.Common.Command, "UPDATE") || Equal
              (import.Common.Command, "DELETE") || Equal
              (import.Common.Command, "DISPLAY"))
            {
              export.Common.Flag = "Y";

              goto ReadEach;
            }

            // **** if command = ADD we need to check if the period for which 
            // the obligation is being added, no obligation exists ****
            if (Equal(import.Common.Command, "ADD"))
            {
              // CQ11647  Check input end date >= existing start date
              if (!Lt(entities.ObligationPaymentSchedule.EndDt,
                import.StartDate.StartDt) && !
                Lt(local.EndDate.Date,
                entities.ObligationPaymentSchedule.StartDt))
              {
                export.Common.Flag = "Y";

                goto ReadEach;
              }
            }
          }
        }

ReadEach:

        if (local.Common.Count == 0)
        {
          export.Common.Flag = "N";
        }

        if (Equal(import.Common.Command, "DISPLAY") && local.Common.Count > 0)
        {
          export.Common.Flag = "Y";
        }

        if ((Equal(import.Common.Command, "UPDATE") || Equal
          (import.Common.Command, "DELETE")) && AsChar(export.Common.Flag) == 'N'
          )
        {
          ExitState = "OBLIGATION_NOT_ACTIVE";
        }

        break;
      default:
        break;
    }
  }

  private bool ReadCsePerson()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorForImportLegDet.Populated = false;
    entities.SuportedForImpLegDet.Populated = false;

    return ReadEach("ReadCsePersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "accountType1", import.HcLapObligorAcctType.AccountType ?? ""
          );
        db.SetNullableString(
          command, "accountType2", import.HcLapSupportedAcctTyp.AccountType ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ObligorForImportLegDet.Number = db.GetString(reader, 0);
        entities.SuportedForImpLegDet.Number = db.GetString(reader, 1);
        entities.ObligorForImportLegDet.Populated = true;
        entities.SuportedForImpLegDet.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 3);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.DayOfMonth2 = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 8);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadObligationObligationPaymentScheduleObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationPaymentSchedule.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach(
      "ReadObligationObligationPaymentScheduleObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
        db.SetString(command, "debtTyp", import.HcOtrnDtAccrualInsruc.DebtType);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.SuportedForImpLegDet.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 6);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 7);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 8);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 9);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 11);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 12);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.ObligationPaymentSchedule.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadObligationObligationPaymentScheduleObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationPaymentSchedule.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach(
      "ReadObligationObligationPaymentScheduleObligationTransaction2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ObligorForImportLegDet.Number);
        db.SetString(command, "debtTyp", import.HcOtrnDtAccrualInsruc.DebtType);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.SuportedForImpLegDet.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 6);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 7);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 8);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 9);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 11);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 12);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.ObligationPaymentSchedule.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationType1()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of BeforeLink.
    /// </summary>
    [JsonPropertyName("beforeLink")]
    public Common BeforeLink
    {
      get => beforeLink ??= new();
      set => beforeLink = value;
    }

    /// <summary>
    /// A value of HcOtrnDtAccrualInsruc.
    /// </summary>
    [JsonPropertyName("hcOtrnDtAccrualInsruc")]
    public ObligationTransaction HcOtrnDtAccrualInsruc
    {
      get => hcOtrnDtAccrualInsruc ??= new();
      set => hcOtrnDtAccrualInsruc = value;
    }

    /// <summary>
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HcLapObligorAcctType.
    /// </summary>
    [JsonPropertyName("hcLapObligorAcctType")]
    public LegalActionPerson HcLapObligorAcctType
    {
      get => hcLapObligorAcctType ??= new();
      set => hcLapObligorAcctType = value;
    }

    /// <summary>
    /// A value of HcLapSupportedAcctTyp.
    /// </summary>
    [JsonPropertyName("hcLapSupportedAcctTyp")]
    public LegalActionPerson HcLapSupportedAcctTyp
    {
      get => hcLapSupportedAcctTyp ??= new();
      set => hcLapSupportedAcctTyp = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public ObligationPaymentSchedule StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public CsePerson Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Common beforeLink;
    private ObligationTransaction hcOtrnDtAccrualInsruc;
    private ObligationTransaction hcOtrnTDebt;
    private LegalActionPerson hcLapObligorAcctType;
    private LegalActionPerson hcLapSupportedAcctTyp;
    private DateWorkArea maximum;
    private DateWorkArea current;
    private Obligation obligation;
    private ObligationPaymentSchedule startDate;
    private Common common;
    private DateWorkArea endDate;
    private LegalActionDetail legalActionDetail;
    private CsePerson concurrent;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ObligationPaymentSchedule Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of ExistingInterstateDebt.
    /// </summary>
    [JsonPropertyName("existingInterstateDebt")]
    public Common ExistingInterstateDebt
    {
      get => existingInterstateDebt ??= new();
      set => existingInterstateDebt = value;
    }

    /// <summary>
    /// A value of AlternateAddress.
    /// </summary>
    [JsonPropertyName("alternateAddress")]
    public Fips AlternateAddress
    {
      get => alternateAddress ??= new();
      set => alternateAddress = value;
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
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private ObligationPaymentSchedule previous;
    private Common existingInterstateDebt;
    private Fips alternateAddress;
    private Obligation obligation;
    private CsePerson obligor;
    private ObligationType obligationType;
    private Common common;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Obligation Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of PathInd.
    /// </summary>
    [JsonPropertyName("pathInd")]
    public Common PathInd
    {
      get => pathInd ??= new();
      set => pathInd = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      active = null;
      endDate = null;
      common = null;
    }

    private Obligation active;
    private DateWorkArea endDate;
    private Common common;
    private Common pathInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of CsePersonAccountSupported.
    /// </summary>
    [JsonPropertyName("csePersonAccountSupported")]
    public CsePersonAccount CsePersonAccountSupported
    {
      get => csePersonAccountSupported ??= new();
      set => csePersonAccountSupported = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LegalActionDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of CsePersonAccountObligor.
    /// </summary>
    [JsonPropertyName("csePersonAccountObligor")]
    public CsePersonAccount CsePersonAccountObligor
    {
      get => csePersonAccountObligor ??= new();
      set => csePersonAccountObligor = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public LegalAction Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorForImportLegDet.
    /// </summary>
    [JsonPropertyName("obligorForImportLegDet")]
    public CsePerson ObligorForImportLegDet
    {
      get => obligorForImportLegDet ??= new();
      set => obligorForImportLegDet = value;
    }

    /// <summary>
    /// A value of SuportedForImpLegDet.
    /// </summary>
    [JsonPropertyName("suportedForImpLegDet")]
    public CsePerson SuportedForImpLegDet
    {
      get => suportedForImpLegDet ??= new();
      set => suportedForImpLegDet = value;
    }

    /// <summary>
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
    }

    /// <summary>
    /// A value of SupportedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("supportedLegalActionPerson")]
    public LegalActionPerson SupportedLegalActionPerson
    {
      get => supportedLegalActionPerson ??= new();
      set => supportedLegalActionPerson = value;
    }

    private CsePerson supportedCsePerson;
    private CsePersonAccount csePersonAccountSupported;
    private LegalActionDetail new1;
    private Fips fips;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction obligationTransaction;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private CsePersonAccount csePersonAccountObligor;
    private ObligationType obligationType;
    private LegalAction existing;
    private Obligation obligation;
    private CsePerson obligorCsePerson;
    private CsePerson obligorForImportLegDet;
    private CsePerson suportedForImpLegDet;
    private LegalActionPerson obligorLegalActionPerson;
    private LegalActionPerson supportedLegalActionPerson;
  }
#endregion
}
