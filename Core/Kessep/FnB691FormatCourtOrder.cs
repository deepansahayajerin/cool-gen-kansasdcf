// Program: FN_B691_FORMAT_COURT_ORDER, ID: 371025803, model: 746.
// Short name: SWE00368
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B691_FORMAT_COURT_ORDER.
/// </summary>
[Serializable]
public partial class FnB691FormatCourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B691_FORMAT_COURT_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB691FormatCourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB691FormatCourtOrder.
  /// </summary>
  public FnB691FormatCourtOrder(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------------------------------------------------------------------------------------------------------------
    // ??/??/??  ??????????			Initial Code
    // 01/23/03  GVandy	PR 162868	Re-coded to correct how old and new court 
    // order number, city,
    // 					and county are derived.
    // --------------------------------------------------------------------------------------------------------------
    // ******************************
    // Get  Court Order information
    // ******************************
    export.CourtOrderRecord.RecordType = "2";
    export.CourtOrderRecord.CourtOrderType = "IVD";
    export.CourtOrderRecord.ModificationDate =
      NumberToString(DateToInt(import.LegalAction.LastModificationReviewDate),
      8, 8);
    export.CourtOrderRecord.StartDate =
      NumberToString(DateToInt(import.LegalAction.FiledDate), 8, 8);

    if (Equal(export.CourtOrderRecord.StartDate, "00000000"))
    {
      export.CourtOrderRecord.StartDate = import.Default1.StartDate;
    }

    export.CourtOrderRecord.EndDate = import.Default1.EndDate;

    // **********************************
    // Format Old Court Order Number
    // **********************************
    if (Lt(0, import.LegalAction.KpcTribunalId))
    {
      if (!ReadTribunal1())
      {
        ExitState = "TRIBUNAL_NF";

        goto Test;
      }

      if (ReadFips())
      {
        local.Fips.Assign(entities.Fips);
      }
      else
      {
        // -- This is not an error.  Foreign tribunals do not have an associated
        // FIPS record.
      }

      if (Equal(local.Fips.StateAbbreviation, "KS"))
      {
        if (local.Fips.Location >= 20 && local.Fips.Location <= 99)
        {
          // -- Kansas Tribal tribunal.
          export.Old.CountyId = local.Fips.CountyAbbreviation ?? Spaces(2);
          export.Old.CourtOrderNumber = import.LegalAction.KpcStandardNo ?? Spaces
            (12);
        }
        else
        {
          // -- Kansas Non Tribal tribunal
          export.Old.CountyId =
            Substring(import.LegalAction.KpcStandardNo, 1, 2);

          // ****************************************************************
          // For multi-tribunal counties the last character of the court case
          // number must correspond to the city as follows:
          // Tribunal				Last Character
          // ---------------------------------
          // 	--------------
          // 669 (Cowley county - Winfield)		W
          // 670 (Cowley county - Arkansas City)	A
          // 671 (Crawford county - Girard)		G
          // 672 (Crawford county - Pittsburg)	P
          // 703 (Labette county - Parsons)		P
          // 704 (Labette county - Oswege)		O
          // 717 (Montgomery county - Independence)	I
          // 718 (Montgomery county - Coffeyville)	C
          // 722 (Neosho county - Chanute)		C
          // 723 (Neosho county - Erie)		E
          // ****************************************************************
          switch(entities.Tribunal.Identifier)
          {
            case 669:
              export.Old.CityIndicator = "W";

              break;
            case 670:
              export.Old.CityIndicator = "A";

              break;
            case 671:
              export.Old.CityIndicator = "G";

              break;
            case 672:
              export.Old.CityIndicator = "P";

              break;
            case 703:
              export.Old.CityIndicator = "P";

              break;
            case 704:
              export.Old.CityIndicator = "O";

              break;
            case 717:
              export.Old.CityIndicator = "I";

              break;
            case 718:
              export.Old.CityIndicator = "C";

              break;
            case 722:
              export.Old.CityIndicator = "C";

              break;
            case 723:
              export.Old.CityIndicator = "E";

              break;
            default:
              break;
          }

          if (CharAt(import.LegalAction.KpcStandardNo, 6) == '*')
          {
            export.Old.CourtOrderNumber =
              Substring(import.LegalAction.KpcStandardNo,
              LegalAction.KpcStandardNo_MaxLength, 3, 3) + " " + Substring
              (import.LegalAction.KpcStandardNo,
              LegalAction.KpcStandardNo_MaxLength, 7, 6);
          }
          else
          {
            export.Old.CourtOrderNumber =
              Substring(import.LegalAction.KpcStandardNo, 3, 10);
          }
        }
      }
      else
      {
        // -- Non Kansas tribunal
        export.Old.CountyId = "IN";
        export.Old.CourtOrderNumber = import.LegalAction.KpcStandardNo ?? Spaces
          (12);
      }
    }

Test:

    // **********************************
    // Format New Court Order Number
    // **********************************
    if (!ReadTribunal2())
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    local.Fips.Assign(local.Null1);

    if (ReadFips())
    {
      local.Fips.Assign(entities.Fips);
    }
    else
    {
      // -- This is not an error.  Foreign tribunals do not have an associated 
      // FIPS record.
    }

    if (Equal(local.Fips.StateAbbreviation, "KS"))
    {
      if (local.Fips.Location >= 20 && local.Fips.Location <= 99)
      {
        // -- Kansas Tribal tribunal.
        export.CourtOrderRecord.CountyId = local.Fips.CountyAbbreviation ?? Spaces
          (2);
        export.CourtOrderRecord.CourtOrderNumber =
          import.LegalAction.StandardNumber ?? Spaces(12);
      }
      else
      {
        // -- Kansas Non Tribal tribunal
        export.CourtOrderRecord.CountyId =
          Substring(import.LegalAction.StandardNumber, 1, 2);

        // ****************************************************************
        // For multi-tribunal counties the last character of the court case
        // number must correspond to the city as follows:
        // Tribunal				Last Character
        // ---------------------------------
        // 	--------------
        // 669 (Cowley county - Winfield)		W
        // 670 (Cowley county - Arkansas City)	A
        // 671 (Crawford county - Girard)		G
        // 672 (Crawford county - Pittsburg)	P
        // 703 (Labette county - Parsons)		P
        // 704 (Labette county - Oswege)		O
        // 717 (Montgomery county - Independence)	I
        // 718 (Montgomery county - Coffeyville)	C
        // 722 (Neosho county - Chanute)		C
        // 723 (Neosho county - Erie)		E
        // ****************************************************************
        switch(entities.Tribunal.Identifier)
        {
          case 669:
            export.CourtOrderRecord.CityIndicator = "W";

            break;
          case 670:
            export.CourtOrderRecord.CityIndicator = "A";

            break;
          case 671:
            export.CourtOrderRecord.CityIndicator = "G";

            break;
          case 672:
            export.CourtOrderRecord.CityIndicator = "P";

            break;
          case 703:
            export.CourtOrderRecord.CityIndicator = "P";

            break;
          case 704:
            export.CourtOrderRecord.CityIndicator = "O";

            break;
          case 717:
            export.CourtOrderRecord.CityIndicator = "I";

            break;
          case 718:
            export.CourtOrderRecord.CityIndicator = "C";

            break;
          case 722:
            export.CourtOrderRecord.CityIndicator = "C";

            break;
          case 723:
            export.CourtOrderRecord.CityIndicator = "E";

            break;
          default:
            break;
        }

        if (CharAt(import.LegalAction.StandardNumber, 6) == '*')
        {
          export.CourtOrderRecord.CourtOrderNumber =
            Substring(import.LegalAction.StandardNumber,
            LegalAction.StandardNumber_MaxLength, 3, 3) + " " + Substring
            (import.LegalAction.StandardNumber,
            LegalAction.StandardNumber_MaxLength, 7, 6);
        }
        else
        {
          export.CourtOrderRecord.CourtOrderNumber =
            Substring(import.LegalAction.StandardNumber, 3, 10);
        }
      }
    }
    else
    {
      // -- Non Kansas tribunal
      export.CourtOrderRecord.CountyId = "IN";
      export.CourtOrderRecord.CourtOrderNumber =
        import.LegalAction.StandardNumber ?? Spaces(12);
    }
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          import.LegalAction.KpcTribunalId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    System.Diagnostics.Debug.Assert(import.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
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
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public CourtOrderRecord Default1
    {
      get => default1 ??= new();
      set => default1 = value;
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

    private CourtOrderRecord default1;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CourtOrderRecord.
    /// </summary>
    [JsonPropertyName("courtOrderRecord")]
    public CourtOrderRecord CourtOrderRecord
    {
      get => courtOrderRecord ??= new();
      set => courtOrderRecord = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public CourtOrderRecord Old
    {
      get => old ??= new();
      set => old = value;
    }

    private CourtOrderRecord courtOrderRecord;
    private CourtOrderRecord old;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Fips Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Find.
    /// </summary>
    [JsonPropertyName("find")]
    public Common Find
    {
      get => find ??= new();
      set => find = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of CurrentRun.
    /// </summary>
    [JsonPropertyName("currentRun")]
    public DateWorkArea CurrentRun
    {
      get => currentRun ??= new();
      set => currentRun = value;
    }

    private Fips null1;
    private Fips fips;
    private Common find;
    private Common length;
    private Common temp;
    private DateWorkArea currentRun;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private Fips fips;
    private Tribunal tribunal;
  }
#endregion
}
