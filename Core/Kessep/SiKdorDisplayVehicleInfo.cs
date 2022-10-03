// Program: SI_KDOR_DISPLAY_VEHICLE_INFO, ID: 1625325281, model: 746.
// Short name: SWE01171
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_KDOR_DISPLAY_VEHICLE_INFO.
/// </summary>
[Serializable]
public partial class SiKdorDisplayVehicleInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_KDOR_DISPLAY_VEHICLE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiKdorDisplayVehicleInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiKdorDisplayVehicleInfo.
  /// </summary>
  public SiKdorDisplayVehicleInfo(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 12/01/18  GVandy	CQ61419		Initial Code.
    // -------------------------------------------------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseSiReadCsePerson();
    export.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    ReadKdorVehicle1();

    foreach(var item in ReadKdorVehicle2())
    {
      ++local.Common.Count;

      if (local.Common.Count == import.VehicleNumber.Count)
      {
        export.VehicleNumber.Count = local.Common.Count;
        export.KdorVehicle.Identifier = entities.KdorVehicle.Identifier;

        break;
      }
    }

    if (!entities.KdorVehicle.Populated)
    {
      ExitState = "KDOR_VEHICLE_NF";

      return;
    }

    export.XofX.Text18 = "Vehicle " + NumberToString
      (export.VehicleNumber.Count, 14, 2) + " of " + NumberToString
      (export.TotalVehicles.Count, 14, 2);

    // For vehicle records the body of the screen is formatted like this...
    //  Date Received from KDOR MM-DD-YYYY
    //  Last Name XXXXXXXXXXXXXXXXX First Name XXXXXXXXXXXX
    //  SSN XXX-XX-XXXX
    //  DOB MM-DD-YYYY
    //  License Number XXXXXXXXX
    //  VIN   XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //  Make  XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //  Model XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //  Year  XXXX
    //  Plate # XXXXXXXXX
    for(export.Export1.Index = 0; export.Export1.Index < 13; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      export.Export1.Update.GexportHighlight.Flag = "N";

      switch(export.Export1.Index + 1)
      {
        case 1:
          // Date Received from KDOR MM-DD-YYYY
          if (Equal(entities.KdorVehicle.LastUpdatedTstamp,
            local.Null1.Timestamp))
          {
            local.Date.Text10 =
              NumberToString(Month(Date(entities.KdorVehicle.CreatedTstamp)),
              14, 2) + "-" + NumberToString
              (Day(Date(entities.KdorVehicle.CreatedTstamp)), 14, 2) + "-" + NumberToString
              (Year(Date(entities.KdorVehicle.CreatedTstamp)), 12, 4);

            if (Lt(Date(entities.KdorVehicle.CreatedTstamp),
              Now().Date.AddDays(-30)))
            {
              export.DeleteEligible.Flag = "Y";
            }
            else
            {
              export.DeleteEligible.Flag = "N";
            }
          }
          else
          {
            local.Date.Text10 =
              NumberToString(
                Month(Date(entities.KdorVehicle.LastUpdatedTstamp)), 14, 2) + "-"
              + NumberToString
              (Day(Date(entities.KdorVehicle.LastUpdatedTstamp)), 14, 2) + "-"
              + NumberToString
              (Year(Date(entities.KdorVehicle.LastUpdatedTstamp)), 12, 4);

            if (Lt(Date(entities.KdorVehicle.LastUpdatedTstamp),
              Now().Date.AddDays(-30)))
            {
              export.DeleteEligible.Flag = "Y";
            }
            else
            {
              export.DeleteEligible.Flag = "N";
            }
          }

          export.Export1.Update.G.Text80 = "Date Received from KDOR " + local
            .Date.Text10;

          break;
        case 2:
          // Spaces
          export.Export1.Update.G.Text80 = "";

          break;
        case 3:
          // Last Name XXXXXXXXXXXXXXXXX First Name XXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "Last Name " + entities
            .KdorVehicle.LastName + " First Name " + entities
            .KdorVehicle.FirstName;

          break;
        case 4:
          // SSN XXX-XX-XXXX
          export.Export1.Update.G.Text80 = "SSN " + Substring
            (entities.KdorVehicle.Ssn, KdorVehicle.Ssn_MaxLength, 1, 3) + "-"
            + Substring
            (entities.KdorVehicle.Ssn, KdorVehicle.Ssn_MaxLength, 4, 2) + "-"
            + Substring
            (entities.KdorVehicle.Ssn, KdorVehicle.Ssn_MaxLength, 6, 4);

          break;
        case 5:
          // DOB MM-DD-YYYY
          local.Date.Text10 =
            NumberToString(Month(entities.KdorVehicle.DateOfBirth), 14, 2) + "-"
            + NumberToString(Day(entities.KdorVehicle.DateOfBirth), 14, 2) + "-"
            + NumberToString(Year(entities.KdorVehicle.DateOfBirth), 12, 4);
          export.Export1.Update.G.Text80 = "DOB " + local.Date.Text10;

          break;
        case 6:
          // License Number XXXXXXXXX
          export.Export1.Update.G.Text80 = "License Number " + entities
            .KdorVehicle.LicenseNumber;

          break;
        case 7:
          // Spaces
          export.Export1.Update.G.Text80 = "";

          break;
        case 8:
          // VIN   XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "VIN     " + entities
            .KdorVehicle.VinNumber;

          break;
        case 9:
          // Make  XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "Make    " + entities
            .KdorVehicle.Make;

          break;
        case 10:
          // Model XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "Model   " + entities
            .KdorVehicle.Model;

          break;
        case 11:
          // Year  XXXX
          export.Export1.Update.G.Text80 = "Year    " + entities
            .KdorVehicle.Year;

          break;
        case 12:
          // Plate # XXXXXXXXX
          export.Export1.Update.G.Text80 = "Plate # " + entities
            .KdorVehicle.PlateNumber;

          break;
        case 13:
          // Spaces
          export.Export1.Update.G.Text80 = "";

          break;
        default:
          break;
      }
    }

    export.Export1.CheckIndex();

    foreach(var item in ReadKdorVehicleOwner())
    {
      // Each vehicle owner is added after the vehicle info and formated like...
      //  Owner <#>:
      //    Organization Name 
      // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //    Last Name 
      // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //    First Name 
      // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //    Middle Name 
      // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //    Suffix XXXXXXXX
      //    Address XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //            XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //            XXXXXXXXXXXXXXXXXXXX XX XXXXXXXXX
      //    Vestment Type XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
      //    Home Phone XXXXXXXXXXXXXXXXXXXXXXXXX
      //    Business Phone XXXXXXXXXXXXXXXXXXXXXXXXX
      for(local.Common.Count = 1; local.Common.Count <= 13; ++
        local.Common.Count)
      {
        export.Export1.Index = export.Export1.Count;
        export.Export1.CheckSize();

        export.Export1.Update.GexportHighlight.Flag = "N";

        switch(local.Common.Count)
        {
          case 1:
            // Owner <#>:
            export.Export1.Update.G.Text80 = "Owner " + NumberToString
              (entities.KdorVehicleOwner.Identifier, 15, 1) + ":";

            break;
          case 2:
            // Organization Name 
            // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Organization Name " + entities
              .KdorVehicleOwner.OrganizationName;

            break;
          case 3:
            // Last Name 
            // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Last Name " + entities
              .KdorVehicleOwner.LastName;

            break;
          case 4:
            // First Name 
            // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  First Name " + entities
              .KdorVehicleOwner.FirstName;

            break;
          case 5:
            // Middle Name 
            // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Middle Name " + entities
              .KdorVehicleOwner.MiddleName;

            break;
          case 6:
            // Suffix XXXXXXXX
            export.Export1.Update.G.Text80 = "  Suffix " + entities
              .KdorVehicleOwner.Suffix;

            break;
          case 7:
            // Address XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Address " + entities
              .KdorVehicleOwner.AddressLine1;

            break;
          case 8:
            // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "          " + entities
              .KdorVehicleOwner.AddressLine2;

            break;
          case 9:
            // XXXXXXXXXXXXXXXXXXXX XX XXXXXXXXX
            export.Export1.Update.G.Text80 = "          " + entities
              .KdorVehicleOwner.City + " " + entities.KdorVehicleOwner.State + " " +
              entities.KdorVehicleOwner.ZipCode;

            break;
          case 10:
            // Vestment Type XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Vestment Type " + entities
              .KdorVehicleOwner.VestmentType;

            break;
          case 11:
            // Home Phone XXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Home Phone " + entities
              .KdorVehicleOwner.HomePhone;

            break;
          case 12:
            // Business Phone XXXXXXXXXXXXXXXXXXXXXXXXX
            export.Export1.Update.G.Text80 = "  Business Phone " + entities
              .KdorVehicleOwner.BusinessPhone;

            break;
          case 13:
            export.Export1.Update.G.Text80 = "";

            break;
          default:
            break;
        }
      }
    }
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 2);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 3);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 4);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 5);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadKdorVehicle1()
  {
    return Read("ReadKdorVehicle1",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        export.TotalVehicles.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadKdorVehicle2()
  {
    entities.KdorVehicle.Populated = false;

    return ReadEach("ReadKdorVehicle2",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KdorVehicle.Identifier = db.GetInt32(reader, 0);
        entities.KdorVehicle.LastName = db.GetNullableString(reader, 1);
        entities.KdorVehicle.FirstName = db.GetNullableString(reader, 2);
        entities.KdorVehicle.Ssn = db.GetNullableString(reader, 3);
        entities.KdorVehicle.DateOfBirth = db.GetNullableDate(reader, 4);
        entities.KdorVehicle.LicenseNumber = db.GetNullableString(reader, 5);
        entities.KdorVehicle.VinNumber = db.GetNullableString(reader, 6);
        entities.KdorVehicle.Make = db.GetNullableString(reader, 7);
        entities.KdorVehicle.Model = db.GetNullableString(reader, 8);
        entities.KdorVehicle.Year = db.GetNullableString(reader, 9);
        entities.KdorVehicle.PlateNumber = db.GetNullableString(reader, 10);
        entities.KdorVehicle.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.KdorVehicle.CreatedBy = db.GetString(reader, 12);
        entities.KdorVehicle.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.KdorVehicle.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.KdorVehicle.FkCktCsePersnumb = db.GetString(reader, 15);
        entities.KdorVehicle.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKdorVehicleOwner()
  {
    System.Diagnostics.Debug.Assert(entities.KdorVehicle.Populated);
    entities.KdorVehicleOwner.Populated = false;

    return ReadEach("ReadKdorVehicleOwner",
      (db, command) =>
      {
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers",
          entities.KdorVehicle.FkCktCsePersnumb);
        db.SetInt32(
          command, "fkCktKdorVehidentifier", entities.KdorVehicle.Identifier);
      },
      (db, reader) =>
      {
        entities.KdorVehicleOwner.Identifier = db.GetInt32(reader, 0);
        entities.KdorVehicleOwner.OrganizationName =
          db.GetNullableString(reader, 1);
        entities.KdorVehicleOwner.FirstName = db.GetNullableString(reader, 2);
        entities.KdorVehicleOwner.MiddleName = db.GetNullableString(reader, 3);
        entities.KdorVehicleOwner.LastName = db.GetNullableString(reader, 4);
        entities.KdorVehicleOwner.Suffix = db.GetNullableString(reader, 5);
        entities.KdorVehicleOwner.AddressLine1 =
          db.GetNullableString(reader, 6);
        entities.KdorVehicleOwner.AddressLine2 =
          db.GetNullableString(reader, 7);
        entities.KdorVehicleOwner.City = db.GetNullableString(reader, 8);
        entities.KdorVehicleOwner.State = db.GetNullableString(reader, 9);
        entities.KdorVehicleOwner.ZipCode = db.GetNullableString(reader, 10);
        entities.KdorVehicleOwner.VestmentType =
          db.GetNullableString(reader, 11);
        entities.KdorVehicleOwner.HomePhone = db.GetNullableString(reader, 12);
        entities.KdorVehicleOwner.BusinessPhone =
          db.GetNullableString(reader, 13);
        entities.KdorVehicleOwner.CreatedTstamp = db.GetDateTime(reader, 14);
        entities.KdorVehicleOwner.CreatedBy = db.GetString(reader, 15);
        entities.KdorVehicleOwner.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.KdorVehicleOwner.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 17);
        entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers =
          db.GetString(reader, 18);
        entities.KdorVehicleOwner.FkCktKdorVehidentifier =
          db.GetInt32(reader, 19);
        entities.KdorVehicleOwner.Populated = true;

        return true;
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
    /// A value of VehicleNumber.
    /// </summary>
    [JsonPropertyName("vehicleNumber")]
    public Common VehicleNumber
    {
      get => vehicleNumber ??= new();
      set => vehicleNumber = value;
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

    private Common vehicleNumber;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportHighlight.
      /// </summary>
      [JsonPropertyName("gexportHighlight")]
      public Common GexportHighlight
      {
        get => gexportHighlight ??= new();
        set => gexportHighlight = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private WorkArea g;
      private Common gexportHighlight;
    }

    /// <summary>
    /// A value of DeleteEligible.
    /// </summary>
    [JsonPropertyName("deleteEligible")]
    public Common DeleteEligible
    {
      get => deleteEligible ??= new();
      set => deleteEligible = value;
    }

    /// <summary>
    /// A value of TotalVehicles.
    /// </summary>
    [JsonPropertyName("totalVehicles")]
    public Common TotalVehicles
    {
      get => totalVehicles ??= new();
      set => totalVehicles = value;
    }

    /// <summary>
    /// A value of VehicleNumber.
    /// </summary>
    [JsonPropertyName("vehicleNumber")]
    public Common VehicleNumber
    {
      get => vehicleNumber ??= new();
      set => vehicleNumber = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
    }

    /// <summary>
    /// A value of XofX.
    /// </summary>
    [JsonPropertyName("xofX")]
    public WorkArea XofX
    {
      get => xofX ??= new();
      set => xofX = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Common deleteEligible;
    private Common totalVehicles;
    private Common vehicleNumber;
    private KdorVehicle kdorVehicle;
    private WorkArea xofX;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
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
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    private DateWorkArea null1;
    private Common common;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea date;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KdorVehicleOwner.
    /// </summary>
    [JsonPropertyName("kdorVehicleOwner")]
    public KdorVehicleOwner KdorVehicleOwner
    {
      get => kdorVehicleOwner ??= new();
      set => kdorVehicleOwner = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
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

    private KdorVehicleOwner kdorVehicleOwner;
    private KdorVehicle kdorVehicle;
    private CsePerson csePerson;
  }
#endregion
}
