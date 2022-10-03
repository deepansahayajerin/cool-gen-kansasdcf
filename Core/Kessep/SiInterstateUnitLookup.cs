// Program: SI_INTERSTATE_UNIT_LOOKUP, ID: 372609896, model: 746.
// Short name: SWE02567
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_INTERSTATE_UNIT_LOOKUP.
/// </summary>
[Serializable]
public partial class SiInterstateUnitLookup: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INTERSTATE_UNIT_LOOKUP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInterstateUnitLookup(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInterstateUnitLookup.
  /// </summary>
  public SiInterstateUnitLookup(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date		Developer	Description
    // 04/26/1999	Carl Ott	Initial Dev.
    // ------------------------------------------------------------
    ReadFips();

    if (!IsEmpty(entities.Fips.StateAbbreviation))
    {
      export.InterstateContactAddress.Street1 = "CSE INTERSTATE UNIT";

      switch(TrimEnd(entities.Fips.StateAbbreviation))
      {
        case "AL":
          export.InterstateContactAddress.Street2 = "50 RIPLEY STREET";
          export.InterstateContactAddress.City = "MONTGOMERY";
          export.InterstateContactAddress.State = "AL";
          export.InterstateContactAddress.ZipCode = "36130";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "AK":
          export.InterstateContactAddress.Street2 = "550 W 7TH AVE, 4TH FL";
          export.InterstateContactAddress.City = "ANCHORAGE";
          export.InterstateContactAddress.State = "AK";
          export.InterstateContactAddress.ZipCode = "99501";
          export.InterstateContactAddress.Zip4 = "6699";

          break;
        case "AR":
          export.InterstateContactAddress.Street2 = "P. O. BOX 8133";
          export.InterstateContactAddress.City = "LITTLE ROCK";
          export.InterstateContactAddress.State = "AR";
          export.InterstateContactAddress.ZipCode = "72203";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "AZ":
          export.InterstateContactAddress.Street2 = "P. O. BOX 3822";
          export.InterstateContactAddress.City = "PHOENIX";
          export.InterstateContactAddress.State = "AZ";
          export.InterstateContactAddress.ZipCode = "85030";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "CA":
          export.InterstateContactAddress.Street2 = "P. O. BOX 944245";
          export.InterstateContactAddress.City = "SACRAMENTO";
          export.InterstateContactAddress.State = "CA";
          export.InterstateContactAddress.ZipCode = "94244";
          export.InterstateContactAddress.Zip4 = "2450";

          break;
        case "CO":
          export.InterstateContactAddress.Street2 = "1575 SHERMAN STREET";
          export.InterstateContactAddress.City = "DENVER";
          export.InterstateContactAddress.State = "CO";
          export.InterstateContactAddress.ZipCode = "80203";
          export.InterstateContactAddress.Zip4 = "1714";

          break;
        case "CT":
          export.InterstateContactAddress.Street2 = "287 MAIN STREET";
          export.InterstateContactAddress.City = "EAST HARTFORD";
          export.InterstateContactAddress.State = "CT";
          export.InterstateContactAddress.ZipCode = "06118";
          export.InterstateContactAddress.Zip4 = "1885";

          break;
        case "DE":
          export.InterstateContactAddress.Street2 = "P. O. BOX 904";
          export.InterstateContactAddress.City = "NEW CASTLE";
          export.InterstateContactAddress.State = "DE";
          export.InterstateContactAddress.ZipCode = "19720";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "FL":
          export.InterstateContactAddress.Street2 = "P. O. BOX 8030";
          export.InterstateContactAddress.City = "TALLAHASSEE";
          export.InterstateContactAddress.State = "FL";
          export.InterstateContactAddress.ZipCode = "32314";
          export.InterstateContactAddress.Zip4 = "8030";

          break;
        case "DC":
          export.InterstateContactAddress.Street2 = "800 9TH STREET, S.W.";
          export.InterstateContactAddress.City = "WASHINGTON";
          export.InterstateContactAddress.State = "DC";
          export.InterstateContactAddress.ZipCode = "20024";
          export.InterstateContactAddress.Zip4 = "2485";

          break;
        case "GA":
          export.InterstateContactAddress.Street2 = "P. O. BOX 38070";
          export.InterstateContactAddress.City = "ATLANTA";
          export.InterstateContactAddress.State = "GA";
          export.InterstateContactAddress.ZipCode = "30334";
          export.InterstateContactAddress.Zip4 = "0070";

          break;
        case "GU":
          export.InterstateContactAddress.Street2 = "#701 PACIFIC NEWS BLDG";
          export.InterstateContactAddress.City = "AGANA";
          export.InterstateContactAddress.State = "GU";
          export.InterstateContactAddress.ZipCode = "96910";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "HI":
          export.InterstateContactAddress.Street2 = "680 IWILEI RD, SUITE 490";
          export.InterstateContactAddress.City = "HONOLULU";
          export.InterstateContactAddress.State = "HI";
          export.InterstateContactAddress.ZipCode = "96817";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "ID":
          export.InterstateContactAddress.Street2 = "P. O. BOX 83720";
          export.InterstateContactAddress.City = "BOISE";
          export.InterstateContactAddress.State = "ID";
          export.InterstateContactAddress.ZipCode = "83720";
          export.InterstateContactAddress.Zip4 = "0036";

          break;
        case "IL":
          export.InterstateContactAddress.Street2 = "P. O. BOX 19405";
          export.InterstateContactAddress.City = "SPRINGFIELD";
          export.InterstateContactAddress.State = "IL";
          export.InterstateContactAddress.ZipCode = "62794";
          export.InterstateContactAddress.Zip4 = "9405";

          break;
        case "IN":
          export.InterstateContactAddress.Street2 = "402 WEST WASHINGTON, W360";
          export.InterstateContactAddress.City = "INDIANAPOLIS";
          export.InterstateContactAddress.State = "IN";
          export.InterstateContactAddress.ZipCode = "46204";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "IA":
          export.InterstateContactAddress.Street2 = "211 E. MAPLE STREET";
          export.InterstateContactAddress.City = "DES MOINES";
          export.InterstateContactAddress.State = "IA";
          export.InterstateContactAddress.ZipCode = "50309";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "KS":
          break;
        case "KY":
          export.InterstateContactAddress.Street2 = "275 E. MAIN STREET";
          export.InterstateContactAddress.City = "FRANKFORT";
          export.InterstateContactAddress.State = "KY";
          export.InterstateContactAddress.ZipCode = "40621";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "LA":
          export.InterstateContactAddress.Street2 = "P. O. BOX 940103";
          export.InterstateContactAddress.City = "BATON ROUGE";
          export.InterstateContactAddress.State = "LA";
          export.InterstateContactAddress.ZipCode = "70804";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "ME":
          export.InterstateContactAddress.Street2 = "STATE HOUSE STATION 11";
          export.InterstateContactAddress.City = "AUGUSTA";
          export.InterstateContactAddress.State = "ME";
          export.InterstateContactAddress.ZipCode = "04333";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "MD":
          export.InterstateContactAddress.Street2 = "311 W. SARATOGA STREET";
          export.InterstateContactAddress.City = "BALTIMORE";
          export.InterstateContactAddress.State = "MD";
          export.InterstateContactAddress.ZipCode = "21201";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "MA":
          export.InterstateContactAddress.Street2 = "P. O. BOX 4068";
          export.InterstateContactAddress.City = "WAKEFIELD";
          export.InterstateContactAddress.State = "MA";
          export.InterstateContactAddress.ZipCode = "01880";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "MI":
          export.InterstateContactAddress.Street2 = "P. O. BOX 30037";
          export.InterstateContactAddress.City = "LANSING";
          export.InterstateContactAddress.State = "MI";
          export.InterstateContactAddress.ZipCode = "48909";
          export.InterstateContactAddress.Zip4 = "7978";

          break;
        case "MN":
          export.InterstateContactAddress.Street2 = "444 LAFAYETTE ROAD";
          export.InterstateContactAddress.City = "ST. PAUL";
          export.InterstateContactAddress.State = "MN";
          export.InterstateContactAddress.ZipCode = "55155";
          export.InterstateContactAddress.Zip4 = "3846";

          break;
        case "MS":
          export.InterstateContactAddress.Street2 = "P. O. BOX 352";
          export.InterstateContactAddress.City = "JACKSON";
          export.InterstateContactAddress.State = "MS";
          export.InterstateContactAddress.ZipCode = "39205";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "MO":
          export.InterstateContactAddress.Street2 = "P. O. BOX 1468";
          export.InterstateContactAddress.City = "JEFFERSON CITY";
          export.InterstateContactAddress.State = "MO";
          export.InterstateContactAddress.ZipCode = "65102";
          export.InterstateContactAddress.Zip4 = "1468";

          break;
        case "MT":
          export.InterstateContactAddress.Street2 = "P. O. BOX 202943";
          export.InterstateContactAddress.City = "HELENA";
          export.InterstateContactAddress.State = "MT";
          export.InterstateContactAddress.ZipCode = "53320";
          export.InterstateContactAddress.Zip4 = "2943";

          break;
        case "NE":
          export.InterstateContactAddress.Street2 = "P. O. BOX 95026";
          export.InterstateContactAddress.City = "LINCOLN";
          export.InterstateContactAddress.State = "NE";
          export.InterstateContactAddress.ZipCode = "68509";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "NV":
          export.InterstateContactAddress.Street2 = "2527 NORTH CARSON ST";
          export.InterstateContactAddress.City = "CARSON CITY";
          export.InterstateContactAddress.State = "NV";
          export.InterstateContactAddress.ZipCode = "89710";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "NH":
          export.InterstateContactAddress.Street2 = "6 HAZEN DRIVE";
          export.InterstateContactAddress.City = "CONCORD";
          export.InterstateContactAddress.State = "NH";
          export.InterstateContactAddress.ZipCode = "03301";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "NJ":
          export.InterstateContactAddress.Street2 = "5 QUAKERBRIDGE PL - CN716";
          export.InterstateContactAddress.City = "MERCERVILLE";
          export.InterstateContactAddress.State = "NJ";
          export.InterstateContactAddress.ZipCode = "08625";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "NC":
          export.InterstateContactAddress.Street2 = "100 E. SIX FORKS ROAD";
          export.InterstateContactAddress.City = "RALEIGH";
          export.InterstateContactAddress.State = "NC";
          export.InterstateContactAddress.ZipCode = "27609";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "NM":
          export.InterstateContactAddress.Street2 = "P. O. BOX 25109";
          export.InterstateContactAddress.City = "SANTA FE";
          export.InterstateContactAddress.State = "NM";
          export.InterstateContactAddress.ZipCode = "87505";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "NY":
          export.InterstateContactAddress.Street2 = "P. O. BOX 125";
          export.InterstateContactAddress.City = "ALBANY";
          export.InterstateContactAddress.State = "NY";
          export.InterstateContactAddress.ZipCode = "12260";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "ND":
          export.InterstateContactAddress.Street2 = "P. O. BOX 7190";
          export.InterstateContactAddress.City = "BISMARCK";
          export.InterstateContactAddress.State = "ND";
          export.InterstateContactAddress.ZipCode = "58507";
          export.InterstateContactAddress.Zip4 = "7190";

          break;
        case "OH":
          export.InterstateContactAddress.Street2 = "30 E. BROAD ST, 31ST FL";
          export.InterstateContactAddress.City = "COLUMBUS";
          export.InterstateContactAddress.State = "OH";
          export.InterstateContactAddress.ZipCode = "43266";
          export.InterstateContactAddress.Zip4 = "0423";

          break;
        case "OK":
          export.InterstateContactAddress.Street2 = "P. O. BOX 53552";
          export.InterstateContactAddress.City = "OKLAHOMA CITY";
          export.InterstateContactAddress.State = "OK";
          export.InterstateContactAddress.ZipCode = "73152";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "OR":
          export.InterstateContactAddress.Street2 = "1495 EDGEWATER NW, ST 290";
          export.InterstateContactAddress.City = "SALEM";
          export.InterstateContactAddress.State = "OR";
          export.InterstateContactAddress.ZipCode = "97304";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "PA":
          export.InterstateContactAddress.Street2 = "P. O. BOX 8018";
          export.InterstateContactAddress.City = "HARRISBURG";
          export.InterstateContactAddress.State = "PA";
          export.InterstateContactAddress.ZipCode = "17105";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "PR":
          export.InterstateContactAddress.Street2 = "11030 PONCE DELEON";
          export.InterstateContactAddress.City = "SAN JUAN";
          export.InterstateContactAddress.State = "PR";
          export.InterstateContactAddress.ZipCode = "00910";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "RI":
          export.InterstateContactAddress.Street2 = "77 DORRANCE STREET";
          export.InterstateContactAddress.City = "PROVIDENCE";
          export.InterstateContactAddress.State = "RI";
          export.InterstateContactAddress.ZipCode = "02903";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "SC":
          export.InterstateContactAddress.Street2 = "P. O. BOX 1469";
          export.InterstateContactAddress.City = "COLUMBIA";
          export.InterstateContactAddress.State = "SC";
          export.InterstateContactAddress.ZipCode = "29202";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "SD":
          export.InterstateContactAddress.Street2 = "700 GOVERNORS DRIVE";
          export.InterstateContactAddress.City = "PIERRE";
          export.InterstateContactAddress.State = "SD";
          export.InterstateContactAddress.ZipCode = "57501";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "TN":
          export.InterstateContactAddress.Street2 = "400 DEADERICK STREET";
          export.InterstateContactAddress.City = "NASHVILLE";
          export.InterstateContactAddress.State = "TN";
          export.InterstateContactAddress.ZipCode = "37248";
          export.InterstateContactAddress.Zip4 = "7400";

          break;
        case "TX":
          export.InterstateContactAddress.Street2 = "P. O. BOX 12017";
          export.InterstateContactAddress.City = "AUSTIN";
          export.InterstateContactAddress.State = "TX";
          export.InterstateContactAddress.ZipCode = "78711";
          export.InterstateContactAddress.Zip4 = "2017";

          break;
        case "UT":
          export.InterstateContactAddress.Street2 = "P. O. BOX 45011";
          export.InterstateContactAddress.City = "SALT LAKE CITY";
          export.InterstateContactAddress.State = "UT";
          export.InterstateContactAddress.ZipCode = "84145";
          export.InterstateContactAddress.Zip4 = "0011";

          break;
        case "VA":
          export.InterstateContactAddress.Street2 = "730 EAST BROAD";
          export.InterstateContactAddress.City = "RICHMOND";
          export.InterstateContactAddress.State = "VA";
          export.InterstateContactAddress.ZipCode = "23219";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "VT":
          export.InterstateContactAddress.Street2 = "103 SOUTH MAIN STREET";
          export.InterstateContactAddress.City = "WATERBURY";
          export.InterstateContactAddress.State = "VT";
          export.InterstateContactAddress.ZipCode = "05676";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "VI":
          export.InterstateContactAddress.Street2 = "# 7 CHARLOTTE AMALIE";
          export.InterstateContactAddress.City = "ST. THOMAS";
          export.InterstateContactAddress.State = "VI";
          export.InterstateContactAddress.ZipCode = "00802";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "WA":
          export.InterstateContactAddress.Street2 = "P. O. BOX 9008";
          export.InterstateContactAddress.City = "OLYMPIA";
          export.InterstateContactAddress.State = "WA";
          export.InterstateContactAddress.ZipCode = "98507";
          export.InterstateContactAddress.Zip4 = "9008";

          break;
        case "WV":
          export.InterstateContactAddress.Street2 = "504 VIRGINIA STREET";
          export.InterstateContactAddress.City = "CHARLESTON";
          export.InterstateContactAddress.State = "WV";
          export.InterstateContactAddress.ZipCode = "25302";
          export.InterstateContactAddress.Zip4 = "";

          break;
        case "WI":
          export.InterstateContactAddress.Street2 = "P. O. BOX 7935";
          export.InterstateContactAddress.City = "MADISON";
          export.InterstateContactAddress.State = "WI";
          export.InterstateContactAddress.ZipCode = "53707";
          export.InterstateContactAddress.Zip4 = "7935";

          break;
        case "WY":
          export.InterstateContactAddress.Street2 = "HATHAWAY BUILDING, RM 385";
          export.InterstateContactAddress.City = "CHEYENNE";
          export.InterstateContactAddress.State = "WY";
          export.InterstateContactAddress.ZipCode = "82002";
          export.InterstateContactAddress.Zip4 = "0490";

          break;
        default:
          break;
      }
    }
    else
    {
      ExitState = "FIPS_NF";
    }
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
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

    private Fips fips;
  }
#endregion
}
