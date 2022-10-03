// Program: FN_AB_DET_IF_OB_EXISTS_FOR_LA, ID: 372095444, model: 746.
// Short name: SWE01610
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
/// A program: FN_AB_DET_IF_OB_EXISTS_FOR_LA.
/// </para>
/// <para>
/// Resp: Finance	
/// Determine if an Obligation Exists for a legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class FnAbDetIfObExistsForLa: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_DET_IF_OB_EXISTS_FOR_LA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbDetIfObExistsForLa(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbDetIfObExistsForLa.
  /// </summary>
  public FnAbDetIfObExistsForLa(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***---  This is used ONLY by ONAC  ---***
    ExitState = "ACO_NN0000_ALL_OK";

    // ================================================
    // 10/2/98  -  B Adams  -  deleted both USE fn-hardcode-debt-distribution  
    // and  USE fn-hardcode-legal; no attributes used here.
    // 10/7/98  -  B Adams  -  deleted Read of Legal_Action.  No data from it 
    // was being used; changed further references to use SOME / THAT logic.
    // 1/22/99  -  B Adams  -  Read properties set
    // 1/03/00  -  K Price - If retired obligations found, set return flag to R
    // ================================================
    export.ActiveObligationFound.Flag = "N";

    if (ReadLegalAction())
    {
      if (ReadLegalActionDetail())
      {
        export.LegalActionDetail.Assign(entities.LegalActionDetail);

        if (import.ObligationType.SystemGeneratedIdentifier == 0)
        {
          if (ReadObligationType2())
          {
            export.ObligationType.Assign(entities.ObligationType);
          }
          else
          {
            ExitState = "OBLIGATION_TYPE_NF";

            return;
          }
        }
        else if (ReadObligationType1())
        {
          export.ObligationType.Assign(entities.ObligationType);
        }
        else
        {
          ExitState = "OBLIGATION_TYPE_NF";

          return;
        }
      }
      else
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        return;
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (IsEmpty(import.CsePerson.Number))
    {
      // <<< RBM   12/05/97 Control will come here if it was a flow from LDET 
      // >>>
      foreach(var item in ReadCsePersonCsePerson2())
      {
        // **** Find out if there is any currently active obligation ****
        local.Common.Count = 0;

        // =================================================
        // 11/13/98 - Bud Adams  -  The following read each was not
        //   properly qualified.
        // =================================================
        foreach(var item1 in ReadObligationObligationTransaction2())
        {
          ++local.Common.Count;
          MoveObligation(entities.Obligation, export.Obligation);
          export.Obligor.Number = entities.ObligorCsePerson.Number;

          // =================================================
          // 2/17/1999 - bud adams  -  Converted debts intentionally have
          //   the Debt_Detail Due_Date set to be the day before Conversion
          //   so, for them, we're just going to grab whatever's there.
          // =================================================
          if (Equal(entities.Obligation.CreatedBy, "CONVERSN"))
          {
            if (ReadDebtDetail3())
            {
              // ************************************************
              // *An active Obligation has been found for the   *
              // * Import Legal Action Identifier.              *
              // ************************************************
              export.ActiveObligationFound.Flag = "Y";

              return;
            }
            else
            {
              // =================================================
              // 4/15/99 - bud adams  -  display the retired date
              // =================================================
              if (ReadDebtDetail4())
              {
                // ************************************************
                // *An inactive Obligation has been found for the *
                // * Import Legal Action Identifier.              *
                // *
                // 
                // *
                // * Get the Retired Date for screen display      *
                // ************************************************
                export.ActiveObligationFound.Flag = "R";

                return;
              }
            }
          }
          else if (ReadDebtDetail1())
          {
            // ************************************************
            // *An active Obligation has been found for the   *
            // * Import Legal Action Identifier.              *
            // ************************************************
            export.ActiveObligationFound.Flag = "Y";

            return;
          }
          else
          {
            // =================================================
            // 4/15/99 - bud adams  -  display the retired date
            // =================================================
            if (ReadDebtDetail2())
            {
              // ************************************************
              // *An inactive Obligation has been found for the *
              // * Import Legal Action Identifier.              *
              // *
              // 
              // *
              // * Get the Retired Date for screen display      *
              // ************************************************
              export.ActiveObligationFound.Flag = "R";

              return;
            }
          }
        }
      }
    }
    else
    {
      // <<< RBM  12/05/97  Control will come here for all subsequent attempts 
      // to REDisplay/Add/Update or if it was a flow from any screen other than
      // LDET >>>
      // **** Find out if there is any currently active obligation ****
      local.Common.Count = 0;

      foreach(var item in ReadCsePersonCsePerson1())
      {
        // ================================================
        // 2/08/00 - K. Price - Qualified read - it would allow Obligations
        // to be found for the same person and Legal Action for a
        // different Legal Action Detail.
        // Added - AND obligation for CURRENT Legal Action Detail
        // ================================================
        foreach(var item1 in ReadObligationObligationTransaction1())
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

          local.Obligation.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;
          ++local.Common.Count;

          // =================================================
          // 2/17/1999 - bud adams  -  Converted debts intentionally have
          //   the Debt_Detail Due_Date set to be the day before Conversion
          //   so, for them, we're just going to grab whatever's there.
          // =================================================
          if (Equal(entities.Obligation.CreatedBy, "CONVERSN"))
          {
            if (ReadDebtDetail3())
            {
              export.ActiveObligationFound.Flag = "Y";

              return;
            }
            else
            {
              export.ActiveObligationFound.Flag = "N";

              // =================================================
              // 4/15/99 - bud adams  -  display the retired date
              // =================================================
              if (ReadDebtDetail4())
              {
                // ************************************************
                // *An inactive Obligation has been found for the *
                // * Import Legal Action Identifier.              *
                // *
                // 
                // *
                // * Get the Retired Date for screen display      *
                // ************************************************
                export.ActiveObligationFound.Flag = "R";

                return;
              }
            }
          }
          else if (ReadDebtDetail1())
          {
            export.ActiveObligationFound.Flag = "Y";

            return;
          }
          else
          {
            export.ActiveObligationFound.Flag = "N";

            // =================================================
            // 4/15/99 - bud adams  -  display the retired date
            // =================================================
            if (ReadDebtDetail2())
            {
              // ************************************************
              // *An inactive Obligation has been found for the *
              // * Import Legal Action Identifier.              *
              // *
              // 
              // *
              // * Get the Retired Date for screen display      *
              // ************************************************
              export.ActiveObligationFound.Flag = "R";

              return;
            }
          }
        }
      }
    }
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTmst = source.CreatedTmst;
  }

  private IEnumerable<bool> ReadCsePersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.SupportedCsePerson.Populated = false;
    entities.ObligorCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
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
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Number = db.GetString(reader, 1);
        entities.SupportedCsePerson.Populated = true;
        entities.ObligorCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.SupportedCsePerson.Populated = false;
    entities.ObligorCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCsePerson2",
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
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Number = db.GetString(reader, 1);
        entities.SupportedCsePerson.Populated = true;
        entities.ObligorCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.RetiredDt.GetValueOrDefault());
        db.SetDate(
          command, "dueDt",
          entities.LegalActionDetail.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.RetiredDt.GetValueOrDefault());
        db.SetDate(
          command, "dueDt",
          entities.LegalActionDetail.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.RetiredDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail4()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail4",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.RetiredDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 3);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 4);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.DayOfMonth2 = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 7);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationTransaction1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.SupportedCsePerson.Number);
        db.SetString(command, "debtTyp", import.HcOtrnDtDebtDetail.DebtType);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CreatedBy = db.GetString(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 10);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 11);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 12);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 14);
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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

  private IEnumerable<bool> ReadObligationObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationTransaction2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.SupportedCsePerson.Number);
        db.SetString(command, "debtTyp", import.HcOtrnDtDebtDetail.DebtType);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CreatedBy = db.GetString(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 10);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 11);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 12);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 14);
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
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
    /// A value of HcLapSupportedAcctTyp.
    /// </summary>
    [JsonPropertyName("hcLapSupportedAcctTyp")]
    public LegalActionPerson HcLapSupportedAcctTyp
    {
      get => hcLapSupportedAcctTyp ??= new();
      set => hcLapSupportedAcctTyp = value;
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
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HcOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hcOtrnDtDebtDetail")]
    public ObligationTransaction HcOtrnDtDebtDetail
    {
      get => hcOtrnDtDebtDetail ??= new();
      set => hcOtrnDtDebtDetail = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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

    private LegalActionPerson hcLapSupportedAcctTyp;
    private LegalActionPerson hcLapObligorAcctType;
    private ObligationTransaction hcOtrnTDebt;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private LegalActionDetail legalActionDetail;
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
    /// A value of ActiveObligationFound.
    /// </summary>
    [JsonPropertyName("activeObligationFound")]
    public Common ActiveObligationFound
    {
      get => activeObligationFound ??= new();
      set => activeObligationFound = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private Common activeObligationFound;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson obligor;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Obltran.
    /// </summary>
    [JsonPropertyName("obltran")]
    public Common Obltran
    {
      get => obltran ??= new();
      set => obltran = value;
    }

    /// <summary>
    /// A value of ObligorForImportLad.
    /// </summary>
    [JsonPropertyName("obligorForImportLad")]
    public CsePerson ObligorForImportLad
    {
      get => obligorForImportLad ??= new();
      set => obligorForImportLad = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DebtDetail Zero
    {
      get => zero ??= new();
      set => zero = value;
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

    private Common obltran;
    private CsePerson obligorForImportLad;
    private DebtDetail zero;
    private Obligation obligation;
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
    /// A value of SupportedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("supportedLegalActionPerson")]
    public LegalActionPerson SupportedLegalActionPerson
    {
      get => supportedLegalActionPerson ??= new();
      set => supportedLegalActionPerson = value;
    }

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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
    }

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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private LegalActionPerson supportedLegalActionPerson;
    private CsePerson supportedCsePerson;
    private CsePersonAccount obligor;
    private CsePerson obligorCsePerson;
    private LegalActionPerson obligorLegalActionPerson;
    private CsePersonAccount supported;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
  }
#endregion
}
