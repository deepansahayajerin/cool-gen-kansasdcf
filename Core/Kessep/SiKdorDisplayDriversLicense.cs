// Program: SI_KDOR_DISPLAY_DRIVERS_LICENSE, ID: 1625325280, model: 746.
// Short name: SWE01170
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_KDOR_DISPLAY_DRIVERS_LICENSE.
/// </summary>
[Serializable]
public partial class SiKdorDisplayDriversLicense: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_KDOR_DISPLAY_DRIVERS_LICENSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiKdorDisplayDriversLicense(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiKdorDisplayDriversLicense.
  /// </summary>
  public SiKdorDisplayDriversLicense(IContext context, Import import,
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

    if (!ReadKdorDriversLicense())
    {
      ExitState = "KDOR_DRIVERS_LICENSE_NF";

      return;
    }

    switch(AsChar(entities.KdorDriversLicense.Type1))
    {
      case 'E':
        // -------------------------------------------------------------------------------------
        // --KDOR Drivers License Match Error
        // -------------------------------------------------------------------------------------
        // For error records the body of the screen is formatted like this...
        //  Date Received from KDOR MM-DD-YYYY
        //  Last Name XXXXXXXXXXXXXXXXX First Name XXXXXXXXXXXX
        //  SSN XXX-XX-XXXX
        //  DOB MM-DD-YYYY
        //  License Number XXXXXXXXX
        //  Status XXX XXXXXXXXXXXX Error Reason XXXXXXXXXXXXXXXXXXXXXXXX
        for(export.Export1.Index = 0; export.Export1.Index < 8; ++
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
              if (Equal(entities.KdorDriversLicense.LastUpdatedTstamp,
                local.Null1.LastUpdatedTstamp))
              {
                local.Date.Text10 =
                  NumberToString(Month(
                    Date(entities.KdorDriversLicense.CreatedTstamp)), 14, 2) + "-"
                  + NumberToString
                  (Day(Date(entities.KdorDriversLicense.CreatedTstamp)), 14, 2) +
                  "-" + NumberToString
                  (Year(Date(entities.KdorDriversLicense.CreatedTstamp)), 12, 4);
                  
              }
              else
              {
                local.Date.Text10 =
                  NumberToString(Month(
                    Date(entities.KdorDriversLicense.LastUpdatedTstamp)), 14,
                  2) + "-" + NumberToString
                  (Day(Date(entities.KdorDriversLicense.LastUpdatedTstamp)), 14,
                  2) + "-" + NumberToString
                  (Year(Date(entities.KdorDriversLicense.LastUpdatedTstamp)),
                  12, 4);
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
                .KdorDriversLicense.LastName + " First Name " + entities
                .KdorDriversLicense.FirstName;

              break;
            case 4:
              // SSN XXX-XX-XXXX
              export.Export1.Update.G.Text80 = "SSN " + Substring
                (entities.KdorDriversLicense.Ssn,
                KdorDriversLicense.Ssn_MaxLength, 1, 3) + "-" + Substring
                (entities.KdorDriversLicense.Ssn,
                KdorDriversLicense.Ssn_MaxLength, 4, 2) + "-" + Substring
                (entities.KdorDriversLicense.Ssn,
                KdorDriversLicense.Ssn_MaxLength, 6, 4);

              break;
            case 5:
              // DOB MM-DD-YYYY
              local.Date.Text10 =
                NumberToString(Month(entities.KdorDriversLicense.DateOfBirth),
                14, 2) + "-" + NumberToString
                (Day(entities.KdorDriversLicense.DateOfBirth), 14, 2) + "-" + NumberToString
                (Year(entities.KdorDriversLicense.DateOfBirth), 12, 4);
              export.Export1.Update.G.Text80 = "DOB " + local.Date.Text10;

              break;
            case 6:
              // License Number XXXXXXXXX
              export.Export1.Update.G.Text80 = "License Number " + entities
                .KdorDriversLicense.LicenseNumber;

              break;
            case 7:
              // Spaces
              export.Export1.Update.G.Text80 = "";

              break;
            case 8:
              // Status XXX XXXXXXXXXXXX Error Reason XXXXXXXXXXXXXXXXXXXXXXXX
              // --Convert status code to a description.
              // --
              // --  Status  	Description
              // --  ------  	-------------------
              // --  CAN		Cancelled
              // --  DED		Deceased
              // --  DIS		Disqualified
              // --  EXP		Expired
              // --  M/R		Moped Revoked
              // --  M/S		Moped Suspended
              // --  OTH		Other
              // --  RES		Restricted
              // --  REV		Revoked
              // --  SUR		Surrendered
              // --  SUS		Suspended
              // --  VAL		Valid
              switch(TrimEnd(entities.KdorDriversLicense.Status))
              {
                case "CAN":
                  local.WorkArea.Text15 = "Cancelled";

                  break;
                case "DED":
                  local.WorkArea.Text15 = "Deceased";

                  break;
                case "DIS":
                  local.WorkArea.Text15 = "Disqualified";

                  break;
                case "EXP":
                  local.WorkArea.Text15 = "Expired";

                  break;
                case "M/R":
                  local.WorkArea.Text15 = "Moped Revoked";

                  break;
                case "M/S":
                  local.WorkArea.Text15 = "Moped Suspended";

                  break;
                case "OTH":
                  local.WorkArea.Text15 = "Other";

                  break;
                case "RES":
                  local.WorkArea.Text15 = "Restricted";

                  break;
                case "REV":
                  local.WorkArea.Text15 = "Revoked";

                  break;
                case "SUR":
                  local.WorkArea.Text15 = "Surrendered";

                  break;
                case "SUS":
                  local.WorkArea.Text15 = "Suspended";

                  break;
                case "VAL":
                  local.WorkArea.Text15 = "Valid";

                  break;
                default:
                  break;
              }

              export.Export1.Update.G.Text80 = "Status " + entities
                .KdorDriversLicense.Status + " " + local.WorkArea.Text15 + " Error Reason " +
                entities.KdorDriversLicense.ErrorReason;

              break;
            default:
              break;
          }
        }

        export.Export1.CheckIndex();

        break;
      case 'M':
        // -------------------------------------------------------------------------------------
        // --KDOR Drivers License Match
        // -------------------------------------------------------------------------------------
        export.CsePersonAddress.Street1 =
          entities.KdorDriversLicense.AddressLine1;
        export.CsePersonAddress.Street2 =
          entities.KdorDriversLicense.AddressLine2;
        export.CsePersonAddress.City = entities.KdorDriversLicense.City;
        export.CsePersonAddress.State = entities.KdorDriversLicense.State;
        export.CsePersonAddress.ZipCode =
          Substring(entities.KdorDriversLicense.ZipCode, 1, 5);
        export.CsePersonAddress.Zip4 =
          Substring(entities.KdorDriversLicense.ZipCode, 6, 4);
        export.CsePersonAddress.Type1 = "R";
        export.CsePersonAddress.Source = "DMV";
        ReadCsePersonAddress();

        // For match records the body of the screen is formatted like this...
        //  Date Received from KDOR MM-DD-YYYY
        //  Last Name XXXXXXXXXXXXXXXXX First Name XXXXXXXXXXXX
        //  SSN XXX-XX-XXXX
        //  DOB MM-DD-YYYY
        //  License Number XXXXXXXXX DL X Motorcycle X CDL X Exp Date MM-DD-YYYY
        //  Gender X XXXXXXXXXXX
        //  Address XXXXXXXXXXXXXXXXXXXX
        //          XXXXXXXXXXXXXXXXXXXX
        //          XXXXXXXXXXXXXXX XX XXXXXXXXX
        //  Height X XX
        //  Weight XXX
        //  Eye Color XXX XXXXXXXXXXX
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
              if (Equal(entities.KdorDriversLicense.LastUpdatedTstamp,
                local.Null1.LastUpdatedTstamp))
              {
                local.Date.Text10 =
                  NumberToString(Month(
                    Date(entities.KdorDriversLicense.CreatedTstamp)), 14, 2) + "-"
                  + NumberToString
                  (Day(Date(entities.KdorDriversLicense.CreatedTstamp)), 14, 2) +
                  "-" + NumberToString
                  (Year(Date(entities.KdorDriversLicense.CreatedTstamp)), 12, 4);
                  
              }
              else
              {
                local.Date.Text10 =
                  NumberToString(Month(
                    Date(entities.KdorDriversLicense.LastUpdatedTstamp)), 14,
                  2) + "-" + NumberToString
                  (Day(Date(entities.KdorDriversLicense.LastUpdatedTstamp)), 14,
                  2) + "-" + NumberToString
                  (Year(Date(entities.KdorDriversLicense.LastUpdatedTstamp)),
                  12, 4);
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
                .KdorDriversLicense.LastName + " First Name " + entities
                .KdorDriversLicense.FirstName;

              if (!Equal(TrimEnd(local.CsePersonsWorkSet.FirstName),
                TrimEnd(entities.KdorDriversLicense.FirstName)) || !
                Equal(TrimEnd(local.CsePersonsWorkSet.LastName),
                TrimEnd(entities.KdorDriversLicense.LastName)))
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 4:
              // SSN XXX-XX-XXXX
              export.Export1.Update.G.Text80 = "SSN " + Substring
                (entities.KdorDriversLicense.Ssn,
                KdorDriversLicense.Ssn_MaxLength, 1, 3) + "-" + Substring
                (entities.KdorDriversLicense.Ssn,
                KdorDriversLicense.Ssn_MaxLength, 4, 2) + "-" + Substring
                (entities.KdorDriversLicense.Ssn,
                KdorDriversLicense.Ssn_MaxLength, 6, 4);

              if (!Equal(local.CsePersonsWorkSet.Ssn,
                entities.KdorDriversLicense.Ssn))
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 5:
              // DOB MM-DD-YYYY
              local.Date.Text10 =
                NumberToString(Month(entities.KdorDriversLicense.DateOfBirth),
                14, 2) + "-" + NumberToString
                (Day(entities.KdorDriversLicense.DateOfBirth), 14, 2) + "-" + NumberToString
                (Year(entities.KdorDriversLicense.DateOfBirth), 12, 4);
              export.Export1.Update.G.Text80 = "DOB " + local.Date.Text10;

              if (!Equal(local.CsePersonsWorkSet.Dob,
                entities.KdorDriversLicense.DateOfBirth))
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 6:
              // License Number XXXXXXXXX DL X Motorcycle X CDL X Exp Date MM-DD
              // -YYYY
              if (AsChar(entities.KdorDriversLicense.DlClassInd) == '1')
              {
                local.Dl.Text1 = "Y";
              }
              else
              {
                local.Dl.Text1 = "";
              }

              if (AsChar(entities.KdorDriversLicense.MotorcycleClassInd) == '1')
              {
                local.Motorcycle.Text1 = "Y";
              }
              else
              {
                local.Motorcycle.Text1 = "";
              }

              if (AsChar(entities.KdorDriversLicense.CdlClassInd) == '1')
              {
                local.Cdl.Text1 = "Y";
              }
              else
              {
                local.Cdl.Text1 = "";
              }

              local.Date.Text10 =
                NumberToString(Month(entities.KdorDriversLicense.ExpirationDt),
                14, 2) + "-" + NumberToString
                (Day(entities.KdorDriversLicense.ExpirationDt), 14, 2) + "-" + NumberToString
                (Year(entities.KdorDriversLicense.ExpirationDt), 12, 4);
              export.Export1.Update.G.Text80 = "License Number " + entities
                .KdorDriversLicense.LicenseNumber + " DL " + local.Dl.Text1 + " Motorcycle " +
                local.Motorcycle.Text1 + " CDL " + local.Cdl.Text1 + " Exp Date " +
                local.Date.Text10;
              local.CsePersonLicense.Number =
                entities.KdorDriversLicense.LicenseNumber;

              if (!ReadCsePersonLicense())
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 7:
              // Gender X XXXXXXXXXXX
              switch(AsChar(entities.KdorDriversLicense.GenderCode))
              {
                case 'U':
                  local.WorkArea.Text15 = "Unknown";

                  break;
                case 'M':
                  local.WorkArea.Text15 = "Male";

                  break;
                case 'F':
                  local.WorkArea.Text15 = "Female";

                  break;
                case 'O':
                  local.WorkArea.Text15 = "Other";

                  break;
                case 'X':
                  local.WorkArea.Text15 = "Unspecified";

                  break;
                default:
                  break;
              }

              export.Export1.Update.G.Text80 = "Gender " + entities
                .KdorDriversLicense.GenderCode + " " + local.WorkArea.Text15;

              if (AsChar(local.CsePersonsWorkSet.Sex) != AsChar
                (entities.KdorDriversLicense.GenderCode))
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 8:
              // Address XXXXXXXXXXXXXXXXXXXX
              export.Export1.Update.G.Text80 = "Address " + entities
                .KdorDriversLicense.AddressLine1;

              if (!entities.CsePersonAddress.Populated)
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 9:
              // XXXXXXXXXXXXXXXXXXXX
              export.Export1.Update.G.Text80 = "        " + entities
                .KdorDriversLicense.AddressLine2;

              if (!entities.CsePersonAddress.Populated)
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 10:
              // XXXXXXXXXXXXXXX XX XXXXXXXXX
              export.Export1.Update.G.Text80 = "        " + entities
                .KdorDriversLicense.City + " " + entities
                .KdorDriversLicense.State + " " + entities
                .KdorDriversLicense.ZipCode;

              if (!entities.CsePersonAddress.Populated)
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 11:
              // Height X XX
              export.Export1.Update.G.Text80 = "Height " + entities
                .KdorDriversLicense.HeightFeet + " " + entities
                .KdorDriversLicense.HeightInches;
              local.ForCompare.HeightFt =
                (int?)StringToNumber(entities.KdorDriversLicense.HeightFeet);
              local.ForCompare.HeightIn =
                (int?)StringToNumber(entities.KdorDriversLicense.HeightInches);

              if (!Equal(local.ForCompare.HeightIn.GetValueOrDefault(),
                entities.CsePerson.HeightIn) || !
                Equal(local.ForCompare.HeightFt.GetValueOrDefault(),
                entities.CsePerson.HeightFt))
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 12:
              // Weight XXX
              export.Export1.Update.G.Text80 = "Weight " + entities
                .KdorDriversLicense.Weight;
              local.ForCompare.Weight =
                (int?)StringToNumber(entities.KdorDriversLicense.Weight);

              if (!Equal(local.ForCompare.Weight.GetValueOrDefault(),
                entities.CsePerson.Weight))
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            case 13:
              // Eye Color XXX XXXXXXXXXXX
              switch(TrimEnd(entities.KdorDriversLicense.EyeColor))
              {
                case "":
                  break;
                case "BLK":
                  local.ForCompare.EyeColor = "BK";
                  local.WorkArea.Text15 = "Black";

                  break;
                case "BLU":
                  local.ForCompare.EyeColor = "BU";
                  local.WorkArea.Text15 = "Blue";

                  break;
                case "BRO":
                  local.ForCompare.EyeColor = "BN";
                  local.WorkArea.Text15 = "Brown";

                  break;
                case "GRY":
                  local.ForCompare.EyeColor = "GY";
                  local.WorkArea.Text15 = "Grey";

                  break;
                case "GRN":
                  local.ForCompare.EyeColor = "GN";
                  local.WorkArea.Text15 = "Green";

                  break;
                case "HAZ":
                  local.ForCompare.EyeColor = "HZ";
                  local.WorkArea.Text15 = "Hazel";

                  break;
                case "MAR":
                  local.ForCompare.EyeColor = "OT";
                  local.WorkArea.Text15 = "Maroon";

                  break;
                case "PNK":
                  local.ForCompare.EyeColor = "OT";
                  local.WorkArea.Text15 = "Pink";

                  break;
                case "DIC":
                  local.ForCompare.EyeColor = "DC";
                  local.WorkArea.Text15 = "Dichromatic";

                  break;
                case "UNK":
                  local.ForCompare.EyeColor = "UN";
                  local.WorkArea.Text15 = "Unknown";

                  break;
                default:
                  local.ForCompare.EyeColor = "OT";
                  local.WorkArea.Text15 = "";

                  break;
              }

              export.Export1.Update.G.Text80 = "Eye Color " + entities
                .KdorDriversLicense.EyeColor + " " + local.WorkArea.Text15;

              if (!Equal(local.ForCompare.EyeColor, entities.CsePerson.EyeColor))
                
              {
                export.Export1.Update.GexportHighlight.Flag = "Y";
              }

              break;
            default:
              break;
          }
        }

        export.Export1.CheckIndex();

        break;
      default:
        break;
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

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(command, "endDate", date);
        db.SetNullableString(
          command, "street1", export.CsePersonAddress.Street1 ?? "");
        db.SetNullableString(
          command, "street2", export.CsePersonAddress.Street2 ?? "");
        db.
          SetNullableString(command, "city", export.CsePersonAddress.City ?? "");
          
        db.SetNullableString(
          command, "state", export.CsePersonAddress.State ?? "");
        db.SetNullableString(
          command, "zipCode", export.CsePersonAddress.ZipCode ?? "");
        db.
          SetNullableString(command, "zip4", export.CsePersonAddress.Zip4 ?? "");
          
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 9);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "numb", local.CsePersonLicense.Number ?? "");
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.IssuingState =
          db.GetNullableString(reader, 2);
        entities.CsePersonLicense.Number = db.GetNullableString(reader, 3);
        entities.CsePersonLicense.ExpirationDt = db.GetNullableDate(reader, 4);
        entities.CsePersonLicense.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonLicense.Populated = true;
      });
  }

  private bool ReadKdorDriversLicense()
  {
    entities.KdorDriversLicense.Populated = false;

    return Read("ReadKdorDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.KdorDriversLicense.Type1 = db.GetString(reader, 0);
        entities.KdorDriversLicense.LastName = db.GetNullableString(reader, 1);
        entities.KdorDriversLicense.FirstName = db.GetNullableString(reader, 2);
        entities.KdorDriversLicense.Ssn = db.GetNullableString(reader, 3);
        entities.KdorDriversLicense.DateOfBirth = db.GetNullableDate(reader, 4);
        entities.KdorDriversLicense.LicenseNumber =
          db.GetNullableString(reader, 5);
        entities.KdorDriversLicense.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.KdorDriversLicense.CreatedBy = db.GetString(reader, 7);
        entities.KdorDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.KdorDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.KdorDriversLicense.Status = db.GetNullableString(reader, 10);
        entities.KdorDriversLicense.ErrorReason =
          db.GetNullableString(reader, 11);
        entities.KdorDriversLicense.DlClassInd =
          db.GetNullableString(reader, 12);
        entities.KdorDriversLicense.MotorcycleClassInd =
          db.GetNullableString(reader, 13);
        entities.KdorDriversLicense.CdlClassInd =
          db.GetNullableString(reader, 14);
        entities.KdorDriversLicense.ExpirationDt =
          db.GetNullableDate(reader, 15);
        entities.KdorDriversLicense.GenderCode =
          db.GetNullableString(reader, 16);
        entities.KdorDriversLicense.AddressLine1 =
          db.GetNullableString(reader, 17);
        entities.KdorDriversLicense.AddressLine2 =
          db.GetNullableString(reader, 18);
        entities.KdorDriversLicense.City = db.GetNullableString(reader, 19);
        entities.KdorDriversLicense.State = db.GetNullableString(reader, 20);
        entities.KdorDriversLicense.ZipCode = db.GetNullableString(reader, 21);
        entities.KdorDriversLicense.HeightFeet =
          db.GetNullableString(reader, 22);
        entities.KdorDriversLicense.HeightInches =
          db.GetNullableString(reader, 23);
        entities.KdorDriversLicense.Weight = db.GetNullableString(reader, 24);
        entities.KdorDriversLicense.EyeColor = db.GetNullableString(reader, 25);
        entities.KdorDriversLicense.FkCktCsePersnumb = db.GetString(reader, 26);
        entities.KdorDriversLicense.Populated = true;
        CheckValid<KdorDriversLicense>("Type1",
          entities.KdorDriversLicense.Type1);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Cdl.
    /// </summary>
    [JsonPropertyName("cdl")]
    public TextWorkArea Cdl
    {
      get => cdl ??= new();
      set => cdl = value;
    }

    /// <summary>
    /// A value of Motorcycle.
    /// </summary>
    [JsonPropertyName("motorcycle")]
    public TextWorkArea Motorcycle
    {
      get => motorcycle ??= new();
      set => motorcycle = value;
    }

    /// <summary>
    /// A value of Dl.
    /// </summary>
    [JsonPropertyName("dl")]
    public TextWorkArea Dl
    {
      get => dl ??= new();
      set => dl = value;
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
    /// A value of ForCompare.
    /// </summary>
    [JsonPropertyName("forCompare")]
    public CsePerson ForCompare
    {
      get => forCompare ??= new();
      set => forCompare = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public KdorDriversLicense Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private TextWorkArea cdl;
    private TextWorkArea motorcycle;
    private TextWorkArea dl;
    private Common common;
    private CsePerson forCompare;
    private CsePersonLicense csePersonLicense;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea workArea;
    private KdorDriversLicense null1;
    private TextWorkArea date;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of KdorDriversLicense.
    /// </summary>
    [JsonPropertyName("kdorDriversLicense")]
    public KdorDriversLicense KdorDriversLicense
    {
      get => kdorDriversLicense ??= new();
      set => kdorDriversLicense = value;
    }

    private CsePersonAddress csePersonAddress;
    private CsePersonLicense csePersonLicense;
    private CsePerson csePerson;
    private KdorDriversLicense kdorDriversLicense;
  }
#endregion
}
