// Program: LE_GET_WORKER_DET_FOR_OBLIGATION, ID: 372601975, model: 746.
// Short name: SWE00780
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
/// A program: LE_GET_WORKER_DET_FOR_OBLIGATION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block returns a repeating group of office service providers 
/// associated with the obligation
/// </para>
/// </summary>
[Serializable]
public partial class LeGetWorkerDetForObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_GET_WORKER_DET_FOR_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeGetWorkerDetForObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeGetWorkerDetForObligation.
  /// </summary>
  public LeGetWorkerDetForObligation(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Date		By		IDCR#
    // ?????		govind
    // Initial code
    // 11/14/97	govind
    // Fixed to look for the latest caserole even if it has been
    // discontinued.
    // 03/23/98	Siraj Konkader
    // ZDEL cleanup
    // 09/25/98	P. Sharp
    // Removed unused Entity actions. Removed read for CSE
    // person, CSE person account, and obligation type and
    // incorporated as part of the qualifier of obligation.
    // 03/23/00	J.Magat      	PR#90394
    // Removed attributes: code and classification from Entity view
    // Obligation_type and use the attributes from Import view
    // instead.
    // 05/02/00	PMcElderry	PR #s 91557, 91696, 91702
    // Incorrect workers and case numbers for certifications; display
    // workers and cases based upon activity of supported
    // 032201   Madhu Kumar           PR#112209.
    // Fixed the display of manual filter which was not working correctly .
    // ----------------------------------------------------------------
    // WR020119    08/27/02   COMPLETE restructure of this A/B
    // Display CASE whether CH/AP has been end-dated or not
    // If Good Cause exist with TYPE other than "CO"
    // - Do Not display that CASE
    // MAX of twelve CASEs - No longer use Table for Worker ID info
    // Display only Open CASEs - Case Status = "O"
    export.ActiveRoleForSupported.SelectChar =
      import.ActiveRoleForSupported.SelectChar;
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);

    if (Equal(import.AsOf.Date, local.Zero.Date))
    {
      local.AsOf.Date = local.Current.Date;
    }
    else
    {
      local.AsOf.Date = import.AsOf.Date;
    }

    // wr010119
    export.One.Number = import.One.Number;
    export.Two.Number = import.Two.Number;
    export.Three.Number = import.Three.Number;
    export.Four.Number = import.Four.Number;
    export.Five.Number = import.Five.Number;
    export.Six.Number = import.Six.Number;
    export.Seven.Number = import.Seven.Number;
    export.Eight.Number = import.Eight.Number;
    export.Nine.Number = import.Nine.Number;
    export.Ten.Number = import.Ten.Number;
    export.Eleven.Number = import.Eleven.Number;
    export.Twelve.Number = import.Twelve.Number;
    export.Group.Index = -1;

    // --------------------------------------------------
    // PR #s 91557, 91696, 91702 - added ADMIN ACT CERT
    // qualifiers; changed READ qualifiers to select only
    // --------------------------------------------------
    if (!ReadObligation())
    {
      ExitState = "OBLIGATION_NF";

      return;
    }

    // *** PR#90394 - Use Attribute from Import View.
    if (AsChar(import.ObligationType.Classification) == 'A')
    {
      // ---------------------------------------------
      // It is an accruing obligation
      // ---------------------------------------------
      local.ObligationTransaction.DebtType = "A";
    }
    else
    {
      // ---------------------------------------------
      // It is a non accruing obligation
      // ---------------------------------------------
      local.ObligationTransaction.DebtType = "D";
    }

    // WR020119
    foreach(var item in ReadDebt())
    {
      if (!ReadCsePerson())
      {
        ExitState = "FN0000_SUPPORTED_PERSON_NF";

        return;
      }

      // *** PR#90394 - Use Attribute from Import View.
      if (Equal(import.ObligationType.Code, "SP") || Equal
        (import.ObligationType.Code, "SAJ"))
      {
        // -----------------------------------------------------------
        // These are spousal support related. We must look only for an
        // AR.
        // -----------------------------------------------------------
        // WR020119 - Retrieve reqardless of whether Role is Active or Not
        foreach(var item1 in ReadCaseRoleCaseCaseRole1())
        {
          // WR020119 - Do NOT display if Good Cause exist
          local.GoodCauseFound.Flag = "N";

          if (ReadGoodCause1())
          {
            // WR020119 - Display if Good Cause Type is "CO"
            if (Equal(entities.GoodCause.Code, "CO"))
            {
              local.GoodCauseFound.Flag = "N";

              goto Read;
            }

            local.GoodCauseFound.Flag = "Y";
          }

Read:

          if (AsChar(local.GoodCauseFound.Flag) == 'Y')
          {
            continue;
          }

          // WR020119 - For each CASE retrieved - display CASE NUMBER
          if (IsEmpty(export.One.Number))
          {
            export.One.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.One.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Two.Number))
          {
            export.Two.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Two.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Three.Number))
          {
            export.Three.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Three.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Four.Number))
          {
            export.Four.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Four.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Five.Number))
          {
            export.Five.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Five.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Six.Number))
          {
            export.Six.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Six.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Seven.Number))
          {
            export.Seven.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Seven.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Eight.Number))
          {
            export.Eight.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Eight.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Nine.Number))
          {
            export.Nine.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Nine.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Ten.Number))
          {
            export.Ten.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Ten.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Eleven.Number))
          {
            export.Eleven.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Eleven.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Twelve.Number))
          {
            export.Twelve.Number = entities.Case1.Number;

            return;
          }

          if (Equal(export.Twelve.Number, entities.Case1.Number))
          {
            continue;
          }

          if (!IsEmpty(export.Twelve.Number))
          {
            return;
          }
        }
      }
      else
      {
        // ---------------------------------------
        // Assume that it is child support related
        // ---------------------------------------
        // ------------------------------------------------------
        // Deliver the case whether child has an
        // active role or Not.
        // ------------------------------------------------------
        // WR020119 - Retrieve reqardless of whether Role is Active or Not
        foreach(var item1 in ReadCaseRoleCaseCaseRole2())
        {
          // WR020119 - Do NOT display if Good Cause exist
          local.GoodCauseFound.Flag = "N";

          foreach(var item2 in ReadGoodCause2())
          {
            // WR020119 - Display if Good Cause Type is "CO"
            if (Equal(entities.GoodCause.Code, "CO"))
            {
              local.GoodCauseFound.Flag = "N";

              break;
            }

            local.GoodCauseFound.Flag = "Y";
          }

          if (AsChar(local.GoodCauseFound.Flag) == 'Y')
          {
            continue;
          }

          // WR020119 - For each CASE retrieved - display CASE NUMBER
          if (IsEmpty(export.One.Number))
          {
            export.One.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.One.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Two.Number))
          {
            export.Two.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Two.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Three.Number))
          {
            export.Three.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Three.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Four.Number))
          {
            export.Four.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Four.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Five.Number))
          {
            export.Five.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Five.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Six.Number))
          {
            export.Six.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Six.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Seven.Number))
          {
            export.Seven.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Seven.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Eight.Number))
          {
            export.Eight.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Eight.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Nine.Number))
          {
            export.Nine.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Nine.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Ten.Number))
          {
            export.Ten.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Ten.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Eleven.Number))
          {
            export.Eleven.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Eleven.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Twelve.Number))
          {
            export.Twelve.Number = entities.Case1.Number;

            return;
          }

          if (Equal(export.Twelve.Number, entities.Case1.Number))
          {
            continue;
          }

          if (!IsEmpty(export.Twelve.Number))
          {
            return;
          }
        }

        // ---------------------------------------
        // Assume that it is NOT child support related
        // ---------------------------------------
        // ------------------------------------------------------
        // Deliver the case whether AP has an
        // active role or Not.
        // ------------------------------------------------------
        // WR020119 - Retrieve reqardless of whether Role is Active or Not
        foreach(var item1 in ReadCaseRoleCaseCaseRole1())
        {
          // WR020119 - Do NOT display if Good Cause exist
          local.GoodCauseFound.Flag = "N";

          foreach(var item2 in ReadGoodCause2())
          {
            // WR020119 - Display if Good Cause Type is "CO"
            if (Equal(entities.GoodCause.Code, "CO"))
            {
              local.GoodCauseFound.Flag = "N";

              break;
            }

            local.GoodCauseFound.Flag = "Y";
          }

          if (AsChar(local.GoodCauseFound.Flag) == 'Y')
          {
            continue;
          }

          // WR020119 - For each CASE retrieved - display CASE NUMBER
          if (IsEmpty(export.One.Number))
          {
            export.One.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.One.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Two.Number))
          {
            export.Two.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Two.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Three.Number))
          {
            export.Three.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Three.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Four.Number))
          {
            export.Four.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Four.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Five.Number))
          {
            export.Five.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Five.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Six.Number))
          {
            export.Six.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Six.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Seven.Number))
          {
            export.Seven.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Seven.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Eight.Number))
          {
            export.Eight.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Eight.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Nine.Number))
          {
            export.Nine.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Nine.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Ten.Number))
          {
            export.Ten.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Ten.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Eleven.Number))
          {
            export.Eleven.Number = entities.Case1.Number;

            continue;
          }

          if (Equal(export.Eleven.Number, entities.Case1.Number))
          {
            continue;
          }

          if (IsEmpty(export.Twelve.Number))
          {
            export.Twelve.Number = entities.Case1.Number;

            return;
          }

          if (Equal(export.Twelve.Number, entities.Case1.Number))
          {
            continue;
          }

          if (!IsEmpty(export.Twelve.Number))
          {
            return;
          }
        }
      }
    }
  }

  private IEnumerable<bool> ReadCaseRoleCaseCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ChildOrAr.Populated = false;
    entities.Ap.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCaseCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", local.AsOf.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", entities.Obligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.ChildOrAr.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.ChildOrAr.CspNumber = db.GetString(reader, 1);
        entities.ChildOrAr.Type1 = db.GetString(reader, 2);
        entities.ChildOrAr.Identifier = db.GetInt32(reader, 3);
        entities.ChildOrAr.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildOrAr.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Ap.CspNumber = db.GetString(reader, 7);
        entities.Ap.Type1 = db.GetString(reader, 8);
        entities.Ap.Identifier = db.GetInt32(reader, 9);
        entities.Ap.StartDate = db.GetNullableDate(reader, 10);
        entities.Ap.EndDate = db.GetNullableDate(reader, 11);
        entities.ChildOrAr.Populated = true;
        entities.Ap.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildOrAr.Type1);
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCaseCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ChildOrAr.Populated = false;
    entities.Ap.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCaseCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", local.AsOf.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", entities.Obligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.ChildOrAr.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.ChildOrAr.CspNumber = db.GetString(reader, 1);
        entities.ChildOrAr.Type1 = db.GetString(reader, 2);
        entities.ChildOrAr.Identifier = db.GetInt32(reader, 3);
        entities.ChildOrAr.StartDate = db.GetNullableDate(reader, 4);
        entities.ChildOrAr.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Ap.CspNumber = db.GetString(reader, 7);
        entities.Ap.Type1 = db.GetString(reader, 8);
        entities.Ap.Identifier = db.GetInt32(reader, 9);
        entities.Ap.StartDate = db.GetNullableDate(reader, 10);
        entities.Ap.EndDate = db.GetNullableDate(reader, 11);
        entities.ChildOrAr.Populated = true;
        entities.Ap.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChildOrAr.Type1);
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
        db.SetNullableString(
          command, "cpaSupType", entities.Debt.CpaSupType ?? "");
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;

    return ReadEach("ReadDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "debtTyp", local.ObligationTransaction.DebtType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadGoodCause1()
  {
    System.Diagnostics.Debug.Assert(entities.Ap.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause1",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", entities.Ap.CasNumber);
        db.SetNullableInt32(command, "croIdentifier1", entities.Ap.Identifier);
        db.SetNullableString(command, "croType1", entities.Ap.Type1);
        db.SetNullableString(command, "cspNumber1", entities.Ap.CspNumber);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private IEnumerable<bool> ReadGoodCause2()
  {
    System.Diagnostics.Debug.Assert(entities.Ap.Populated);
    entities.GoodCause.Populated = false;

    return ReadEach("ReadGoodCause2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", entities.Ap.CasNumber);
        db.SetNullableInt32(command, "croIdentifier1", entities.Ap.Identifier);
        db.SetNullableString(command, "croType1", entities.Ap.Type1);
        db.SetNullableString(command, "cspNumber1", entities.Ap.CspNumber);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);

        return true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
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
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("detailOfficeServiceProvider")]
      public OfficeServiceProvider DetailOfficeServiceProvider
      {
        get => detailOfficeServiceProvider ??= new();
        set => detailOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of InactRoleForSupp.
      /// </summary>
      [JsonPropertyName("inactRoleForSupp")]
      public Common InactRoleForSupp
      {
        get => inactRoleForSupp ??= new();
        set => inactRoleForSupp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private OfficeServiceProvider detailOfficeServiceProvider;
      private ServiceProvider detailServiceProvider;
      private Office detailOffice;
      private Case1 detailCase;
      private Common inactRoleForSupp;
    }

    /// <summary>
    /// A value of One.
    /// </summary>
    [JsonPropertyName("one")]
    public Case1 One
    {
      get => one ??= new();
      set => one = value;
    }

    /// <summary>
    /// A value of Two.
    /// </summary>
    [JsonPropertyName("two")]
    public Case1 Two
    {
      get => two ??= new();
      set => two = value;
    }

    /// <summary>
    /// A value of Three.
    /// </summary>
    [JsonPropertyName("three")]
    public Case1 Three
    {
      get => three ??= new();
      set => three = value;
    }

    /// <summary>
    /// A value of Four.
    /// </summary>
    [JsonPropertyName("four")]
    public Case1 Four
    {
      get => four ??= new();
      set => four = value;
    }

    /// <summary>
    /// A value of Five.
    /// </summary>
    [JsonPropertyName("five")]
    public Case1 Five
    {
      get => five ??= new();
      set => five = value;
    }

    /// <summary>
    /// A value of Six.
    /// </summary>
    [JsonPropertyName("six")]
    public Case1 Six
    {
      get => six ??= new();
      set => six = value;
    }

    /// <summary>
    /// A value of Seven.
    /// </summary>
    [JsonPropertyName("seven")]
    public Case1 Seven
    {
      get => seven ??= new();
      set => seven = value;
    }

    /// <summary>
    /// A value of Eight.
    /// </summary>
    [JsonPropertyName("eight")]
    public Case1 Eight
    {
      get => eight ??= new();
      set => eight = value;
    }

    /// <summary>
    /// A value of Nine.
    /// </summary>
    [JsonPropertyName("nine")]
    public Case1 Nine
    {
      get => nine ??= new();
      set => nine = value;
    }

    /// <summary>
    /// A value of Ten.
    /// </summary>
    [JsonPropertyName("ten")]
    public Case1 Ten
    {
      get => ten ??= new();
      set => ten = value;
    }

    /// <summary>
    /// A value of Eleven.
    /// </summary>
    [JsonPropertyName("eleven")]
    public Case1 Eleven
    {
      get => eleven ??= new();
      set => eleven = value;
    }

    /// <summary>
    /// A value of Twelve.
    /// </summary>
    [JsonPropertyName("twelve")]
    public Case1 Twelve
    {
      get => twelve ??= new();
      set => twelve = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of ActiveRoleForSupported.
    /// </summary>
    [JsonPropertyName("activeRoleForSupported")]
    public Common ActiveRoleForSupported
    {
      get => activeRoleForSupported ??= new();
      set => activeRoleForSupported = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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
    /// A value of AsOf.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateWorkArea AsOf
    {
      get => asOf ??= new();
      set => asOf = value;
    }

    private Case1 one;
    private Case1 two;
    private Case1 three;
    private Case1 four;
    private Case1 five;
    private Case1 six;
    private Case1 seven;
    private Case1 eight;
    private Case1 nine;
    private Case1 ten;
    private Case1 eleven;
    private Case1 twelve;
    private Array<GroupGroup> group;
    private Common activeRoleForSupported;
    private AdministrativeActCertification administrativeActCertification;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson obligor;
    private DateWorkArea asOf;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("detailOfficeServiceProvider")]
      public OfficeServiceProvider DetailOfficeServiceProvider
      {
        get => detailOfficeServiceProvider ??= new();
        set => detailOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of InactRoleForSupp.
      /// </summary>
      [JsonPropertyName("inactRoleForSupp")]
      public Common InactRoleForSupp
      {
        get => inactRoleForSupp ??= new();
        set => inactRoleForSupp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private OfficeServiceProvider detailOfficeServiceProvider;
      private ServiceProvider detailServiceProvider;
      private Office detailOffice;
      private Case1 detailCase;
      private Common inactRoleForSupp;
    }

    /// <summary>
    /// A value of One.
    /// </summary>
    [JsonPropertyName("one")]
    public Case1 One
    {
      get => one ??= new();
      set => one = value;
    }

    /// <summary>
    /// A value of Two.
    /// </summary>
    [JsonPropertyName("two")]
    public Case1 Two
    {
      get => two ??= new();
      set => two = value;
    }

    /// <summary>
    /// A value of Three.
    /// </summary>
    [JsonPropertyName("three")]
    public Case1 Three
    {
      get => three ??= new();
      set => three = value;
    }

    /// <summary>
    /// A value of Four.
    /// </summary>
    [JsonPropertyName("four")]
    public Case1 Four
    {
      get => four ??= new();
      set => four = value;
    }

    /// <summary>
    /// A value of Five.
    /// </summary>
    [JsonPropertyName("five")]
    public Case1 Five
    {
      get => five ??= new();
      set => five = value;
    }

    /// <summary>
    /// A value of Six.
    /// </summary>
    [JsonPropertyName("six")]
    public Case1 Six
    {
      get => six ??= new();
      set => six = value;
    }

    /// <summary>
    /// A value of Seven.
    /// </summary>
    [JsonPropertyName("seven")]
    public Case1 Seven
    {
      get => seven ??= new();
      set => seven = value;
    }

    /// <summary>
    /// A value of Eight.
    /// </summary>
    [JsonPropertyName("eight")]
    public Case1 Eight
    {
      get => eight ??= new();
      set => eight = value;
    }

    /// <summary>
    /// A value of Nine.
    /// </summary>
    [JsonPropertyName("nine")]
    public Case1 Nine
    {
      get => nine ??= new();
      set => nine = value;
    }

    /// <summary>
    /// A value of Ten.
    /// </summary>
    [JsonPropertyName("ten")]
    public Case1 Ten
    {
      get => ten ??= new();
      set => ten = value;
    }

    /// <summary>
    /// A value of Eleven.
    /// </summary>
    [JsonPropertyName("eleven")]
    public Case1 Eleven
    {
      get => eleven ??= new();
      set => eleven = value;
    }

    /// <summary>
    /// A value of Twelve.
    /// </summary>
    [JsonPropertyName("twelve")]
    public Case1 Twelve
    {
      get => twelve ??= new();
      set => twelve = value;
    }

    /// <summary>
    /// A value of ActiveRoleForSupported.
    /// </summary>
    [JsonPropertyName("activeRoleForSupported")]
    public Common ActiveRoleForSupported
    {
      get => activeRoleForSupported ??= new();
      set => activeRoleForSupported = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Case1 one;
    private Case1 two;
    private Case1 three;
    private Case1 four;
    private Case1 five;
    private Case1 six;
    private Case1 seven;
    private Case1 eight;
    private Case1 nine;
    private Case1 ten;
    private Case1 eleven;
    private Case1 twelve;
    private Common activeRoleForSupported;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ActiveGroup group.</summary>
    [Serializable]
    public class ActiveGroup
    {
      /// <summary>
      /// A value of ActiveLocalDetailOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("activeLocalDetailOfficeServiceProvider")]
      public OfficeServiceProvider ActiveLocalDetailOfficeServiceProvider
      {
        get => activeLocalDetailOfficeServiceProvider ??= new();
        set => activeLocalDetailOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of ActiveLocalDetailServiceProvider.
      /// </summary>
      [JsonPropertyName("activeLocalDetailServiceProvider")]
      public ServiceProvider ActiveLocalDetailServiceProvider
      {
        get => activeLocalDetailServiceProvider ??= new();
        set => activeLocalDetailServiceProvider = value;
      }

      /// <summary>
      /// A value of ActiveLocalDetailOffice.
      /// </summary>
      [JsonPropertyName("activeLocalDetailOffice")]
      public Office ActiveLocalDetailOffice
      {
        get => activeLocalDetailOffice ??= new();
        set => activeLocalDetailOffice = value;
      }

      /// <summary>
      /// A value of ActiveLocalDetailCase.
      /// </summary>
      [JsonPropertyName("activeLocalDetailCase")]
      public Case1 ActiveLocalDetailCase
      {
        get => activeLocalDetailCase ??= new();
        set => activeLocalDetailCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private OfficeServiceProvider activeLocalDetailOfficeServiceProvider;
      private ServiceProvider activeLocalDetailServiceProvider;
      private Office activeLocalDetailOffice;
      private Case1 activeLocalDetailCase;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of InactiveLocalDetailOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("inactiveLocalDetailOfficeServiceProvider")]
      public OfficeServiceProvider InactiveLocalDetailOfficeServiceProvider
      {
        get => inactiveLocalDetailOfficeServiceProvider ??= new();
        set => inactiveLocalDetailOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of InactiveLocalDetailServiceProvider.
      /// </summary>
      [JsonPropertyName("inactiveLocalDetailServiceProvider")]
      public ServiceProvider InactiveLocalDetailServiceProvider
      {
        get => inactiveLocalDetailServiceProvider ??= new();
        set => inactiveLocalDetailServiceProvider = value;
      }

      /// <summary>
      /// A value of InactiveLocalDetailOffice.
      /// </summary>
      [JsonPropertyName("inactiveLocalDetailOffice")]
      public Office InactiveLocalDetailOffice
      {
        get => inactiveLocalDetailOffice ??= new();
        set => inactiveLocalDetailOffice = value;
      }

      /// <summary>
      /// A value of InactiveLocalDetailCase.
      /// </summary>
      [JsonPropertyName("inactiveLocalDetailCase")]
      public Case1 InactiveLocalDetailCase
      {
        get => inactiveLocalDetailCase ??= new();
        set => inactiveLocalDetailCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private OfficeServiceProvider inactiveLocalDetailOfficeServiceProvider;
      private ServiceProvider inactiveLocalDetailServiceProvider;
      private Office inactiveLocalDetailOffice;
      private Case1 inactiveLocalDetailCase;
    }

    /// <summary>
    /// A value of GoodCauseFound.
    /// </summary>
    [JsonPropertyName("goodCauseFound")]
    public Common GoodCauseFound
    {
      get => goodCauseFound ??= new();
      set => goodCauseFound = value;
    }

    /// <summary>
    /// Gets a value of Active.
    /// </summary>
    [JsonIgnore]
    public Array<ActiveGroup> Active => active ??= new(ActiveGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Active for json serialization.
    /// </summary>
    [JsonPropertyName("active")]
    [Computed]
    public IList<ActiveGroup> Active_Json
    {
      get => active;
      set => Active.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of ResetSubscript.
    /// </summary>
    [JsonPropertyName("resetSubscript")]
    public Common ResetSubscript
    {
      get => resetSubscript ??= new();
      set => resetSubscript = value;
    }

    /// <summary>
    /// A value of InactiveRoleForSupptd.
    /// </summary>
    [JsonPropertyName("inactiveRoleForSupptd")]
    public Common InactiveRoleForSupptd
    {
      get => inactiveRoleForSupptd ??= new();
      set => inactiveRoleForSupptd = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of AsOf.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateWorkArea AsOf
    {
      get => asOf ??= new();
      set => asOf = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public Case1 Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private Common goodCauseFound;
    private Array<ActiveGroup> active;
    private Array<GroupGroup> group;
    private Common resetSubscript;
    private Common inactiveRoleForSupptd;
    private DateWorkArea max;
    private Case1 case1;
    private DateWorkArea zero;
    private DateWorkArea asOf;
    private DateWorkArea current;
    private ObligationTransaction obligationTransaction;
    private CsePerson csePerson;
    private Common common;
    private Case1 zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdminActionCertObligation.
    /// </summary>
    [JsonPropertyName("adminActionCertObligation")]
    public AdminActionCertObligation AdminActionCertObligation
    {
      get => adminActionCertObligation ??= new();
      set => adminActionCertObligation = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ChildOrAr.
    /// </summary>
    [JsonPropertyName("childOrAr")]
    public CaseRole ChildOrAr
    {
      get => childOrAr ??= new();
      set => childOrAr = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
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
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
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
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    private AdminActionCertObligation adminActionCertObligation;
    private AdministrativeAction administrativeAction;
    private AdministrativeActCertification administrativeActCertification;
    private CsePersonAccount supported1;
    private ObligationTransaction debt;
    private CsePerson supported2;
    private ObligationType obligationType;
    private Obligation obligation;
    private CaseRole childOrAr;
    private CaseRole ap;
    private Case1 case1;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson obligorCsePerson;
    private GoodCause goodCause;
  }
#endregion
}
