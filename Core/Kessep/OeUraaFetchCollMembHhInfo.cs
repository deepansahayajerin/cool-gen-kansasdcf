// Program: OE_URAA_FETCH_COLL_MEMB_HH_INFO, ID: 374460329, model: 746.
// Short name: SWE02533
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_URAA_FETCH_COLL_MEMB_HH_INFO.
/// </summary>
[Serializable]
public partial class OeUraaFetchCollMembHhInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAA_FETCH_COLL_MEMB_HH_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUraaFetchCollMembHhInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUraaFetchCollMembHhInfo.
  /// </summary>
  public OeUraaFetchCollMembHhInfo(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	      CHG REQ# DESCRIPTION
    // Madhu Kumar      05-16-2000   Initial Code
    // Fangman          08-04-2000   Added code for oldest debt & redid I/O for 
    // efficiency.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    MoveCsePersonsWorkSet(import.Member, export.Member);
    MoveDateWorkArea(import.ForAdjustments, export.ForAdjustments);

    if (ReadImHousehold())
    {
      if (ReadCsePerson())
      {
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
        ExitState = "OE0000_CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      ExitState = "OE0000_AE_CASE_NBR_NOT_KNOWN";

      return;
    }

    foreach(var item in ReadImHouseholdMbrMnthlySum2())
    {
      if (export.FirstAfGrant.Year == 0)
      {
        if (Lt(0, entities.ImHouseholdMbrMnthlySum.GrantAmount))
        {
          export.FirstAfGrant.Year = entities.ImHouseholdMbrMnthlySum.Year;
          export.FirstAfGrant.Month = entities.ImHouseholdMbrMnthlySum.Month;
        }
      }

      if (export.FirstMedGrant.Year == 0)
      {
        if (Lt(0, entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount))
        {
          export.FirstMedGrant.Year = entities.ImHouseholdMbrMnthlySum.Year;
          export.FirstMedGrant.Month = entities.ImHouseholdMbrMnthlySum.Month;
        }
      }

      if (export.FirstAfGrant.Year > 0 && export.FirstMedGrant.Year > 0)
      {
        break;
      }
    }

    if (import.ForAdjustments.Month == 0 || import.ForAdjustments.Year == 0)
    {
      // ****************************************************************
      //  If the month and year are not populated upon entry
      //  to the screen we read the most recent household
      //  of MBR mthly sum for the cse person and use that
      //  month and year as the default.
      // ****************************************************************
      if (ReadImHouseholdMbrMnthlySum1())
      {
        export.ForAdjustments.Year = entities.ImHouseholdMbrMnthlySum.Year;
        export.ForAdjustments.Month = entities.ImHouseholdMbrMnthlySum.Month;
      }

      if (export.ForAdjustments.Year == 0)
      {
        return;
      }
    }

    ExitState = "OE0000_NO_ADJSTMTS_FND_FOR_DATA";

    foreach(var item in ReadImHouseholdMbrMnthlySumCsePerson())
    {
      // ****  Totals for person and household for specific year & month.
      if (Equal(entities.ForSummingTotals.Number, entities.CsePerson.Number) &&
        entities.ImHouseholdMbrMnthlySum.Year == export.ForAdjustments.Year && entities
        .ImHouseholdMbrMnthlySum.Month == export.ForAdjustments.Month)
      {
        export.ImHouseholdMbrMnthlySum.Relationship =
          entities.ImHouseholdMbrMnthlySum.Relationship;
        export.Tot.MbrAfGrant.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          GrantAmount.GetValueOrDefault();
        export.Tot.MbrAfUra.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          UraAmount.GetValueOrDefault();
        export.Tot.MbrMedGrant.TotalCurrency += entities.
          ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault();
        export.Tot.MbrMedUra.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          UraMedicalAmount.GetValueOrDefault();
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // ****  Totals for person and household for all years & months.
      if (Equal(entities.ForSummingTotals.Number, entities.CsePerson.Number))
      {
        export.Tot.TotMbrAfGrant.TotalCurrency += entities.
          ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault();
        export.Tot.TotMbrAfUra.TotalCurrency += entities.
          ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
        export.Tot.TotMbrMedGrant.TotalCurrency += entities.
          ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault();
        export.Tot.TotMbrMedUra.TotalCurrency += entities.
          ImHouseholdMbrMnthlySum.UraMedicalAmount.GetValueOrDefault();
      }

      // ****  Totals for household for specific year & month.
      if (entities.ImHouseholdMbrMnthlySum.Year == export
        .ForAdjustments.Year && entities.ImHouseholdMbrMnthlySum.Month == export
        .ForAdjustments.Month)
      {
        export.Tot.HhAfGrant.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          GrantAmount.GetValueOrDefault();
        export.Tot.HhAfUra.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          UraAmount.GetValueOrDefault();
        export.Tot.HhMedGrant.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          GrantMedicalAmount.GetValueOrDefault();
        export.Tot.HhMedUra.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
          UraMedicalAmount.GetValueOrDefault();
      }

      // ****  Totals for household for all years & months.
      export.Tot.TotHhAfGrant.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
        GrantAmount.GetValueOrDefault();
      export.Tot.TotHhAfUra.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
        UraAmount.GetValueOrDefault();
      export.Tot.TotHhMedGrant.TotalCurrency += entities.
        ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault();
      export.Tot.TotHhMedUra.TotalCurrency += entities.ImHouseholdMbrMnthlySum.
        UraMedicalAmount.GetValueOrDefault();
    }

    foreach(var item in ReadUraCollectionApplicationImHouseholdMbrMnthlySum())
    {
      // ****  Totals for person and household for specific year & month.
      if (Equal(entities.ForSummingTotals.Number, entities.CsePerson.Number) &&
        entities.ImHouseholdMbrMnthlySum.Year == export.ForAdjustments.Year && entities
        .ImHouseholdMbrMnthlySum.Month == export.ForAdjustments.Month)
      {
        if (AsChar(entities.UraCollectionApplication.Type1) == 'A')
        {
          export.Tot.MbrAfColl.TotalCurrency += entities.
            UraCollectionApplication.CollectionAmountApplied;
        }
        else
        {
          export.Tot.MbrMedColl.TotalCurrency += entities.
            UraCollectionApplication.CollectionAmountApplied;
        }
      }

      // ****  Totals for person and household for all years & months.
      if (Equal(entities.ForSummingTotals.Number, entities.CsePerson.Number))
      {
        if (AsChar(entities.UraCollectionApplication.Type1) == 'A')
        {
          export.Tot.TotMbrAfColl.TotalCurrency += entities.
            UraCollectionApplication.CollectionAmountApplied;
        }
        else
        {
          export.Tot.TotMbrMedColl.TotalCurrency += entities.
            UraCollectionApplication.CollectionAmountApplied;
        }
      }

      // ****  Totals for household for specific year & month.
      if (entities.ImHouseholdMbrMnthlySum.Year == export
        .ForAdjustments.Year && entities.ImHouseholdMbrMnthlySum.Month == export
        .ForAdjustments.Month)
      {
        if (AsChar(entities.UraCollectionApplication.Type1) == 'A')
        {
          export.Tot.HhAfColl.TotalCurrency += entities.
            UraCollectionApplication.CollectionAmountApplied;
        }
        else
        {
          export.Tot.HhMedColl.TotalCurrency += entities.
            UraCollectionApplication.CollectionAmountApplied;
        }
      }

      // ****  Totals for household for all years & months.
      if (AsChar(entities.UraCollectionApplication.Type1) == 'A')
      {
        export.Tot.TotHhAfColl.TotalCurrency += entities.
          UraCollectionApplication.CollectionAmountApplied;
      }
      else
      {
        export.Tot.TotHhMedColl.TotalCurrency += entities.
          UraCollectionApplication.CollectionAmountApplied;
      }
    }

    foreach(var item in ReadImHouseholdMbrMnthlyAdjImHouseholdMbrMnthlySum())
    {
      // ****  Totals for household for specific year & month.
      if (Equal(entities.ForSummingTotals.Number, entities.CsePerson.Number) &&
        entities.ImHouseholdMbrMnthlySum.Year == export.ForAdjustments.Year && entities
        .ImHouseholdMbrMnthlySum.Month == export.ForAdjustments.Month)
      {
        if (AsChar(entities.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          export.Tot.MbrAfAdj.TotalCurrency += entities.ImHouseholdMbrMnthlyAdj.
            AdjustmentAmount;
        }
        else
        {
          export.Tot.MbrMedAdj.TotalCurrency += entities.
            ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
        }
      }

      // ****  Totals for person and household for all years & months.
      if (Equal(entities.ForSummingTotals.Number, entities.CsePerson.Number))
      {
        if (AsChar(entities.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          export.Tot.TotMbrAfAdj.TotalCurrency += entities.
            ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
        }
        else
        {
          export.Tot.TotMbrMedAdj.TotalCurrency += entities.
            ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
        }
      }

      // ****  Totals for person and household for specific year & month.
      if (entities.ImHouseholdMbrMnthlySum.Year == export
        .ForAdjustments.Year && entities.ImHouseholdMbrMnthlySum.Month == export
        .ForAdjustments.Month)
      {
        if (AsChar(entities.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          export.Tot.HhAfAdj.TotalCurrency += entities.ImHouseholdMbrMnthlyAdj.
            AdjustmentAmount;
        }
        else
        {
          export.Tot.HhMedAdj.TotalCurrency += entities.ImHouseholdMbrMnthlyAdj.
            AdjustmentAmount;
        }
      }

      // ****  Totals for household for all years & months.
      if (AsChar(entities.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
      {
        export.Tot.TotHhAfAdj.TotalCurrency += entities.ImHouseholdMbrMnthlyAdj.
          AdjustmentAmount;
      }
      else
      {
        export.Tot.TotHhMedAdj.TotalCurrency += entities.
          ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Month = source.Month;
    target.Year = source.Year;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.Member.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Member);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Member.Number);
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

  private IEnumerable<bool> ReadImHouseholdMbrMnthlyAdjImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.ImHouseholdMbrMnthlyAdj.Populated = false;
    entities.ForSummingTotals.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlyAdjImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlyAdj.Type1 = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount =
          db.GetDecimal(reader, 1);
        entities.ImHouseholdMbrMnthlyAdj.CreatedTmst =
          db.GetDateTime(reader, 2);
        entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlyAdj.CspNumber = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 4);
        entities.ForSummingTotals.Number = db.GetString(reader, 4);
        entities.ForSummingTotals.Number = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlyAdj.ImsMonth = db.GetInt32(reader, 5);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 5);
        entities.ImHouseholdMbrMnthlyAdj.ImsYear = db.GetInt32(reader, 6);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 6);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 11);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.ImHouseholdMbrMnthlyAdj.Populated = true;
        entities.ForSummingTotals.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum1()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
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
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 8);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySum2()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySum2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
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
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 8);
        entities.ImHouseholdMbrMnthlySum.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySumCsePerson()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.ForSummingTotals.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySumCsePerson",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
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
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 8);
        entities.ForSummingTotals.Number = db.GetString(reader, 8);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.ForSummingTotals.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadUraCollectionApplicationImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.UraCollectionApplication.Populated = false;
    entities.ForSummingTotals.Populated = false;

    return ReadEach("ReadUraCollectionApplicationImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.UraCollectionApplication.CollectionAmountApplied =
          db.GetDecimal(reader, 0);
        entities.UraCollectionApplication.CspNumber = db.GetString(reader, 1);
        entities.UraCollectionApplication.CpaType = db.GetString(reader, 2);
        entities.UraCollectionApplication.OtyIdentifier =
          db.GetInt32(reader, 3);
        entities.UraCollectionApplication.ObgIdentifier =
          db.GetInt32(reader, 4);
        entities.UraCollectionApplication.OtrIdentifier =
          db.GetInt32(reader, 5);
        entities.UraCollectionApplication.OtrType = db.GetString(reader, 6);
        entities.UraCollectionApplication.CstIdentifier =
          db.GetInt32(reader, 7);
        entities.UraCollectionApplication.CrvIdentifier =
          db.GetInt32(reader, 8);
        entities.UraCollectionApplication.CrtIdentifier =
          db.GetInt32(reader, 9);
        entities.UraCollectionApplication.CrdIdentifier =
          db.GetInt32(reader, 10);
        entities.UraCollectionApplication.ColIdentifier =
          db.GetInt32(reader, 11);
        entities.UraCollectionApplication.ImhAeCaseNo =
          db.GetString(reader, 12);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 12);
        entities.UraCollectionApplication.CspNumber0 = db.GetString(reader, 13);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 13);
        entities.ForSummingTotals.Number = db.GetString(reader, 13);
        entities.ForSummingTotals.Number = db.GetString(reader, 13);
        entities.UraCollectionApplication.ImsMonth = db.GetInt32(reader, 14);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 14);
        entities.UraCollectionApplication.ImsYear = db.GetInt32(reader, 15);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 15);
        entities.UraCollectionApplication.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.UraCollectionApplication.Type1 =
          db.GetNullableString(reader, 17);
        entities.ImHouseholdMbrMnthlySum.Relationship =
          db.GetString(reader, 18);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 19);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 20);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 21);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 22);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.UraCollectionApplication.Populated = true;
        entities.ForSummingTotals.Populated = true;

        return true;
      });
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePersonsWorkSet Member
    {
      get => member ??= new();
      set => member = value;
    }

    /// <summary>
    /// A value of ForAdjustments.
    /// </summary>
    [JsonPropertyName("forAdjustments")]
    public DateWorkArea ForAdjustments
    {
      get => forAdjustments ??= new();
      set => forAdjustments = value;
    }

    private ImHousehold imHousehold;
    private CsePersonsWorkSet member;
    private DateWorkArea forAdjustments;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A TotGroup group.</summary>
    [Serializable]
    public class TotGroup
    {
      /// <summary>
      /// A value of MbrAfGrant.
      /// </summary>
      [JsonPropertyName("mbrAfGrant")]
      public Common MbrAfGrant
      {
        get => mbrAfGrant ??= new();
        set => mbrAfGrant = value;
      }

      /// <summary>
      /// A value of MbrAfColl.
      /// </summary>
      [JsonPropertyName("mbrAfColl")]
      public Common MbrAfColl
      {
        get => mbrAfColl ??= new();
        set => mbrAfColl = value;
      }

      /// <summary>
      /// A value of MbrAfAdj.
      /// </summary>
      [JsonPropertyName("mbrAfAdj")]
      public Common MbrAfAdj
      {
        get => mbrAfAdj ??= new();
        set => mbrAfAdj = value;
      }

      /// <summary>
      /// A value of MbrAfUra.
      /// </summary>
      [JsonPropertyName("mbrAfUra")]
      public Common MbrAfUra
      {
        get => mbrAfUra ??= new();
        set => mbrAfUra = value;
      }

      /// <summary>
      /// A value of MbrMedGrant.
      /// </summary>
      [JsonPropertyName("mbrMedGrant")]
      public Common MbrMedGrant
      {
        get => mbrMedGrant ??= new();
        set => mbrMedGrant = value;
      }

      /// <summary>
      /// A value of MbrMedColl.
      /// </summary>
      [JsonPropertyName("mbrMedColl")]
      public Common MbrMedColl
      {
        get => mbrMedColl ??= new();
        set => mbrMedColl = value;
      }

      /// <summary>
      /// A value of MbrMedAdj.
      /// </summary>
      [JsonPropertyName("mbrMedAdj")]
      public Common MbrMedAdj
      {
        get => mbrMedAdj ??= new();
        set => mbrMedAdj = value;
      }

      /// <summary>
      /// A value of MbrMedUra.
      /// </summary>
      [JsonPropertyName("mbrMedUra")]
      public Common MbrMedUra
      {
        get => mbrMedUra ??= new();
        set => mbrMedUra = value;
      }

      /// <summary>
      /// A value of HhAfGrant.
      /// </summary>
      [JsonPropertyName("hhAfGrant")]
      public Common HhAfGrant
      {
        get => hhAfGrant ??= new();
        set => hhAfGrant = value;
      }

      /// <summary>
      /// A value of HhAfColl.
      /// </summary>
      [JsonPropertyName("hhAfColl")]
      public Common HhAfColl
      {
        get => hhAfColl ??= new();
        set => hhAfColl = value;
      }

      /// <summary>
      /// A value of HhAfAdj.
      /// </summary>
      [JsonPropertyName("hhAfAdj")]
      public Common HhAfAdj
      {
        get => hhAfAdj ??= new();
        set => hhAfAdj = value;
      }

      /// <summary>
      /// A value of HhAfUra.
      /// </summary>
      [JsonPropertyName("hhAfUra")]
      public Common HhAfUra
      {
        get => hhAfUra ??= new();
        set => hhAfUra = value;
      }

      /// <summary>
      /// A value of HhMedGrant.
      /// </summary>
      [JsonPropertyName("hhMedGrant")]
      public Common HhMedGrant
      {
        get => hhMedGrant ??= new();
        set => hhMedGrant = value;
      }

      /// <summary>
      /// A value of HhMedColl.
      /// </summary>
      [JsonPropertyName("hhMedColl")]
      public Common HhMedColl
      {
        get => hhMedColl ??= new();
        set => hhMedColl = value;
      }

      /// <summary>
      /// A value of HhMedAdj.
      /// </summary>
      [JsonPropertyName("hhMedAdj")]
      public Common HhMedAdj
      {
        get => hhMedAdj ??= new();
        set => hhMedAdj = value;
      }

      /// <summary>
      /// A value of HhMedUra.
      /// </summary>
      [JsonPropertyName("hhMedUra")]
      public Common HhMedUra
      {
        get => hhMedUra ??= new();
        set => hhMedUra = value;
      }

      /// <summary>
      /// A value of TotMbrAfGrant.
      /// </summary>
      [JsonPropertyName("totMbrAfGrant")]
      public Common TotMbrAfGrant
      {
        get => totMbrAfGrant ??= new();
        set => totMbrAfGrant = value;
      }

      /// <summary>
      /// A value of TotMbrAfColl.
      /// </summary>
      [JsonPropertyName("totMbrAfColl")]
      public Common TotMbrAfColl
      {
        get => totMbrAfColl ??= new();
        set => totMbrAfColl = value;
      }

      /// <summary>
      /// A value of TotMbrAfAdj.
      /// </summary>
      [JsonPropertyName("totMbrAfAdj")]
      public Common TotMbrAfAdj
      {
        get => totMbrAfAdj ??= new();
        set => totMbrAfAdj = value;
      }

      /// <summary>
      /// A value of TotMbrAfUra.
      /// </summary>
      [JsonPropertyName("totMbrAfUra")]
      public Common TotMbrAfUra
      {
        get => totMbrAfUra ??= new();
        set => totMbrAfUra = value;
      }

      /// <summary>
      /// A value of TotMbrMedGrant.
      /// </summary>
      [JsonPropertyName("totMbrMedGrant")]
      public Common TotMbrMedGrant
      {
        get => totMbrMedGrant ??= new();
        set => totMbrMedGrant = value;
      }

      /// <summary>
      /// A value of TotMbrMedColl.
      /// </summary>
      [JsonPropertyName("totMbrMedColl")]
      public Common TotMbrMedColl
      {
        get => totMbrMedColl ??= new();
        set => totMbrMedColl = value;
      }

      /// <summary>
      /// A value of TotMbrMedAdj.
      /// </summary>
      [JsonPropertyName("totMbrMedAdj")]
      public Common TotMbrMedAdj
      {
        get => totMbrMedAdj ??= new();
        set => totMbrMedAdj = value;
      }

      /// <summary>
      /// A value of TotMbrMedUra.
      /// </summary>
      [JsonPropertyName("totMbrMedUra")]
      public Common TotMbrMedUra
      {
        get => totMbrMedUra ??= new();
        set => totMbrMedUra = value;
      }

      /// <summary>
      /// A value of TotHhAfGrant.
      /// </summary>
      [JsonPropertyName("totHhAfGrant")]
      public Common TotHhAfGrant
      {
        get => totHhAfGrant ??= new();
        set => totHhAfGrant = value;
      }

      /// <summary>
      /// A value of TotHhAfColl.
      /// </summary>
      [JsonPropertyName("totHhAfColl")]
      public Common TotHhAfColl
      {
        get => totHhAfColl ??= new();
        set => totHhAfColl = value;
      }

      /// <summary>
      /// A value of TotHhAfAdj.
      /// </summary>
      [JsonPropertyName("totHhAfAdj")]
      public Common TotHhAfAdj
      {
        get => totHhAfAdj ??= new();
        set => totHhAfAdj = value;
      }

      /// <summary>
      /// A value of TotHhAfUra.
      /// </summary>
      [JsonPropertyName("totHhAfUra")]
      public Common TotHhAfUra
      {
        get => totHhAfUra ??= new();
        set => totHhAfUra = value;
      }

      /// <summary>
      /// A value of TotHhMedGrant.
      /// </summary>
      [JsonPropertyName("totHhMedGrant")]
      public Common TotHhMedGrant
      {
        get => totHhMedGrant ??= new();
        set => totHhMedGrant = value;
      }

      /// <summary>
      /// A value of TotHhMedColl.
      /// </summary>
      [JsonPropertyName("totHhMedColl")]
      public Common TotHhMedColl
      {
        get => totHhMedColl ??= new();
        set => totHhMedColl = value;
      }

      /// <summary>
      /// A value of TotHhMedAdj.
      /// </summary>
      [JsonPropertyName("totHhMedAdj")]
      public Common TotHhMedAdj
      {
        get => totHhMedAdj ??= new();
        set => totHhMedAdj = value;
      }

      /// <summary>
      /// A value of TotHhMedUra.
      /// </summary>
      [JsonPropertyName("totHhMedUra")]
      public Common TotHhMedUra
      {
        get => totHhMedUra ??= new();
        set => totHhMedUra = value;
      }

      private Common mbrAfGrant;
      private Common mbrAfColl;
      private Common mbrAfAdj;
      private Common mbrAfUra;
      private Common mbrMedGrant;
      private Common mbrMedColl;
      private Common mbrMedAdj;
      private Common mbrMedUra;
      private Common hhAfGrant;
      private Common hhAfColl;
      private Common hhAfAdj;
      private Common hhAfUra;
      private Common hhMedGrant;
      private Common hhMedColl;
      private Common hhMedAdj;
      private Common hhMedUra;
      private Common totMbrAfGrant;
      private Common totMbrAfColl;
      private Common totMbrAfAdj;
      private Common totMbrAfUra;
      private Common totMbrMedGrant;
      private Common totMbrMedColl;
      private Common totMbrMedAdj;
      private Common totMbrMedUra;
      private Common totHhAfGrant;
      private Common totHhAfColl;
      private Common totHhAfAdj;
      private Common totHhAfUra;
      private Common totHhMedGrant;
      private Common totHhMedColl;
      private Common totHhMedAdj;
      private Common totHhMedUra;
    }

    /// <summary>
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePersonsWorkSet Member
    {
      get => member ??= new();
      set => member = value;
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
    /// A value of ForAdjustments.
    /// </summary>
    [JsonPropertyName("forAdjustments")]
    public DateWorkArea ForAdjustments
    {
      get => forAdjustments ??= new();
      set => forAdjustments = value;
    }

    /// <summary>
    /// A value of FirstAfGrant.
    /// </summary>
    [JsonPropertyName("firstAfGrant")]
    public DateWorkArea FirstAfGrant
    {
      get => firstAfGrant ??= new();
      set => firstAfGrant = value;
    }

    /// <summary>
    /// A value of FirstMedGrant.
    /// </summary>
    [JsonPropertyName("firstMedGrant")]
    public DateWorkArea FirstMedGrant
    {
      get => firstMedGrant ??= new();
      set => firstMedGrant = value;
    }

    /// <summary>
    /// Gets a value of Tot.
    /// </summary>
    [JsonPropertyName("tot")]
    public TotGroup Tot
    {
      get => tot ?? (tot = new());
      set => tot = value;
    }

    private CsePersonsWorkSet member;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private DateWorkArea forAdjustments;
    private DateWorkArea firstAfGrant;
    private DateWorkArea firstMedGrant;
    private TotGroup tot;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
    }

    /// <summary>
    /// A value of ForSummingTotals.
    /// </summary>
    [JsonPropertyName("forSummingTotals")]
    public CsePerson ForSummingTotals
    {
      get => forSummingTotals ??= new();
      set => forSummingTotals = value;
    }

    private ImHousehold imHousehold;
    private CsePerson csePerson;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private UraCollectionApplication uraCollectionApplication;
    private CsePerson forSummingTotals;
  }
#endregion
}
