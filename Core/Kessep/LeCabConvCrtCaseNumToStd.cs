// Program: LE_CAB_CONV_CRT_CASE_NUM_TO_STD, ID: 371985173, model: 746.
// Short name: SWE01519
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_CONV_CRT_CASE_NUM_TO_STD.
/// </para>
/// <para>
/// This cab converts the Legal Action court case
/// number into the standard number format.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabConvCrtCaseNumToStd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_CONV_CRT_CASE_NUM_TO_STD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabConvCrtCaseNumToStd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabConvCrtCaseNumToStd.
  /// </summary>
  public LeCabConvCrtCaseNumToStd(IContext context, Import import, Export export)
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
    // *** Purpose: Formulate the STANDARD NUMBER using the COURT CASE NUMBER.  
    // The standard number cannot exceed 12 characters.
    // *** Maintenance Log
    // ---------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  -----------	----------	
    // -----------------------------------------------------------------
    // 05/31/96  Henry Hooks			Initial Code.
    // 05/01/97  govind			Added code to convert interstate court case numbers.
    // 03/10/98  rcg	        H00039919	Fix for standard no TD change
    // 05/29/00  JMAGAT	PR#96185	Reformat Standard Numbers If 1-character 
    // Case_Designator,
    // 					pad with trailing "*".
    // 09/15/00  GVandy	PR#102557	The standard number for Indian tribunals 
    // should be the
    // 					same as the court case number.
    // 10/03/00  GVandy	WR 210		New rules for building standard numbers.
    // 08/20/01  GVandy	WR 10346	Allow 'B' Class standard numbers to be in any 
    // format.
    // 05/06/03  GVandy	PR126879	Restructured and added support for foreign 
    // court orders.
    // ---------------------------------------------------------------------------------------------------------
    // 05/15/17  JHarden     CQ 55817   Prevent staff from adding the standard 
    // court order number on LACT.
    export.LegalAction.CourtCaseNumber = import.LegalAction.CourtCaseNumber;

    if (IsEmpty(import.LegalAction.CourtCaseNumber))
    {
      ExitState = "LE0000_COURT_CASE_NO_RQD";

      return;
    }

    if (import.Tribunal.Identifier == 0)
    {
      ExitState = "LE0000_MUST_SELECT_TRIBUNAL";

      return;
    }

    if (!ReadTribunal())
    {
      ExitState = "LE0000_TRIBUNAL_NF";

      return;
    }

    if (!ReadFips())
    {
      if (!ReadFipsTribAddress())
      {
        ExitState = "SI0000_TRIB_FIPS_NF_RB";

        return;
      }
    }

    // -- Determine the type of court that issued the legal action.
    if (AsChar(import.LegalAction.Classification) == 'B')
    {
      // (FYI - the legal action classification does not actually identify the 
      // tribunal as a Bankruptcy court.
      // But for the purposes of deriving a standard number, we treat it as 
      // though is does.)
      local.SpTextWorkArea.Text20 = "BANKRUPTCY COURT";
    }
    else if (entities.Fips.Populated)
    {
      if (entities.Fips.Location >= 20 && entities.Fips.Location <= 99)
      {
        local.SpTextWorkArea.Text20 = "TRIBAL COURT";
      }
      else if (Equal(entities.Fips.StateAbbreviation, "KS"))
      {
        local.SpTextWorkArea.Text20 = "KANSAS COURT";
      }
      else
      {
        local.SpTextWorkArea.Text20 = "OUT OF STATE COURT";
      }
    }
    else if (entities.FipsTribAddress.Populated)
    {
      local.SpTextWorkArea.Text20 = "FOREIGN COURT";
    }

    switch(TrimEnd(local.SpTextWorkArea.Text20))
    {
      case "BANKRUPTCY COURT":
        // The standard number for Bankruptcy actions should be the same as the 
        // court case number.
        export.LegalAction.StandardNumber =
          Substring(import.LegalAction.CourtCaseNumber, 1, 12);

        break;
      case "FOREIGN COURT":
        // The standard number for foreign (i.e. non US) court cases should be 
        // <F><2 digit country abbreviation><9 digit court case number>.
        // (FYI - "G" is also valid for the first character of foreign court 
        // orders, but we only build it as a "F".  The
        // user can change the standard number to "G" if the "F" results in a 
        // duplicate standard number.)
        export.LegalAction.StandardNumber = "F" + entities
          .FipsTribAddress.Country + Substring
          (import.LegalAction.CourtCaseNumber, 17, 1, 9);

        break;
      case "KANSAS COURT":
        // The standard number for Kansas court cases should be
        // <2 digit county abbreviation><2 digit year><2 digit case type 
        // designator><6 digit zero padded court case suffix number>.
        // *** Get starting position of Case_Designator, i.e. alpha,  non-blank
        // [Also known as Case_Type]..
        local.PositionOfCaseType.TotalInteger =
          Verify(export.LegalAction.CourtCaseNumber, " 0123456789");

        if (local.PositionOfCaseType.TotalInteger == 0)
        {
          ExitState = "LE0000_ERROR_DERIVING_STANDRD_NO";

          return;
        }

        // *** Prefix contents before the case_designator.  This is the year the
        // court order was issued.
        local.CourtOrderYearAaWork.TextValue =
          Substring(export.LegalAction.CourtCaseNumber, 1,
          (int)(local.PositionOfCaseType.TotalInteger - 1));

        if (IsEmpty(local.CourtOrderYearAaWork.TextValue))
        {
          ExitState = "LE0000_ERROR_DERIVING_STANDRD_NO";

          return;
        }

        // *** Contents [Suffix] after prefix including the case_designator.
        local.CaseTypeWithSuffix.TextLength16 =
          Substring(export.LegalAction.CourtCaseNumber, 17,
          (int)local.PositionOfCaseType.TotalInteger,
          (int)(Length(TrimEnd(export.LegalAction.CourtCaseNumber)) - local
          .PositionOfCaseType.TotalInteger + 1));

        // *** Size of Contents [Suffix] after prefix including the 
        // case_designator.
        local.LengthOfCaseTypeSuffix.TotalInteger =
          Length(TrimEnd(local.CaseTypeWithSuffix.TextLength16));

        if (local.LengthOfCaseTypeSuffix.TotalInteger == 0)
        {
          ExitState = "LE0000_ERROR_DERIVING_STANDRD_NO";

          return;
        }

        // *** Get starting position of suffix after Case_designator, i.e. 
        // numeric value.
        local.PositionOfOrderSuffix.TotalInteger =
          Verify(local.CaseTypeWithSuffix.TextLength16,
          " ABCDEFGHIJKLMNOPQRSTUVWXYZ*");

        if (local.PositionOfOrderSuffix.TotalInteger == 0)
        {
          ExitState = "LE0000_ERROR_DERIVING_STANDRD_NO";

          return;
        }

        // *** Contents [Value] of Case_Designator only, i.e. alpha only.
        local.CaseType.TextValue =
          Substring(local.CaseTypeWithSuffix.TextLength16, 1,
          (int)(local.PositionOfOrderSuffix.TotalInteger - 1));

        // *** Remaining Characters after Case_Designator.
        local.OldSuffixOfOrderNumber.TextLength16 =
          Substring(local.CaseTypeWithSuffix.TextLength16,
          (int)local.PositionOfOrderSuffix.TotalInteger,
          (int)(local.LengthOfCaseTypeSuffix.TotalInteger - local
          .PositionOfOrderSuffix.TotalInteger + 1));

        // *** Size of Remaining Characters after Case_Designator.
        local.LengthOrderNumberSuffix.TotalInteger =
          Length(TrimEnd(local.OldSuffixOfOrderNumber.TextLength16));

        if (local.LengthOrderNumberSuffix.TotalInteger == 0)
        {
          ExitState = "LE0000_ERROR_DERIVING_STANDRD_NO";

          return;
        }

        // ------------------------------------------------------------
        // 10/03/00	GVandy		WR 210
        // New rules for building standard numbers.
        // ------------------------------------------------------------
        // If court year is between 76 and 99
        local.CourtOrderYearCommon.Count =
          (int)StringToNumber(local.CourtOrderYearAaWork.TextValue);

        if (local.CourtOrderYearCommon.Count >= 76 && local
          .CourtOrderYearCommon.Count <= 99)
        {
          if (Equal(entities.Fips.CountyAbbreviation, "DG") || Equal
            (entities.Fips.CountyAbbreviation, "SG") || Equal
            (entities.Fips.CountyAbbreviation, "SN"))
          {
            // -- The following rules do not apply to Douglas, Segwick, and 
            // Shawnee counties.
            goto Test;
          }

          switch(TrimEnd(TrimEnd(local.CaseType.TextValue)))
          {
            case "C":
              local.CaseType.TextValue = "C*";

              break;
            case "CR":
              local.CaseType.TextValue = "CR";

              break;
            case "CRJ":
              local.CaseType.TextValue = "JV";

              break;
            case "CRM":
              local.CaseType.TextValue = "CR";

              break;
            case "CV":
              local.CaseType.TextValue = "C*";

              break;
            case "CVC":
              local.CaseType.TextValue = "C*";

              break;
            case "CVD":
              local.CaseType.TextValue = "D*";

              break;
            case "CVR":
              local.CaseType.TextValue = "R*";

              break;
            case "D":
              local.CaseType.TextValue = "D*";

              break;
            case "DC":
              local.CaseType.TextValue = "D*";

              break;
            case "DM":
              local.CaseType.TextValue = "D*";

              break;
            case "DP":
              local.CaseType.TextValue = "D*";

              break;
            case "DR":
              local.CaseType.TextValue = "D*";

              break;
            case "DV":
              local.CaseType.TextValue = "D*";

              break;
            case "FS":
              local.CaseType.TextValue = "R*";

              break;
            case "GC":
              local.CaseType.TextValue = "P*";

              break;
            case "J":
              local.CaseType.TextValue = "JV";

              break;
            case "JC":
              local.CaseType.TextValue = "JC";

              break;
            case "JV":
              local.CaseType.TextValue = "JV";

              break;
            case "P":
              local.CaseType.TextValue = "P*";

              break;
            case "PA":
              local.CaseType.TextValue = "D*";

              break;
            case "PR":
              local.CaseType.TextValue = "P*";

              break;
            case "R":
              local.CaseType.TextValue = "R*";

              break;
            case "RC":
              local.CaseType.TextValue = "R*";

              break;
            case "RFSO":
              local.CaseType.TextValue = "FS";

              break;
            case "RO":
              local.CaseType.TextValue = "R*";

              break;
            case "TJ":
              local.CaseType.TextValue = "C*";

              break;
            case "U":
              local.CaseType.TextValue = "R*";

              break;
            case "UC":
              local.CaseType.TextValue = "R*";

              break;
            default:
              // -- Don't convert the case type.
              break;
          }
        }

Test:

        // *** Case_designator minimum size = 2.  If 1-char, pad with trailing "
        // *".
        if (Length(TrimEnd(local.CaseType.TextValue)) == 1)
        {
          local.CaseType.TextValue = TrimEnd(local.CaseType.TextValue) + "*";
        }

        // *** Number of available digits = 12 [max size] - length of prefix 
        // before case_designator -
        //     length of county abbr - length of case_designator.
        local.SizeAvailableOnNumber.Count = 12 - Length
          (TrimEnd(local.CourtOrderYearAaWork.TextValue)) - Length
          (TrimEnd(entities.Fips.CountyAbbreviation)) - Length
          (TrimEnd(local.CaseType.TextValue));

        if (local.LengthOrderNumberSuffix.TotalInteger >= local
          .SizeAvailableOnNumber.Count)
        {
          // *** If suffix exceeds/equal max digits, truncate/use available 
          // digits.
          local.NewSuffixOfOrderNumber.TextLength16 =
            Substring(local.OldSuffixOfOrderNumber.TextLength16, 1,
            local.SizeAvailableOnNumber.Count);
        }
        else
        {
          // *** If suffix is less than max digits, set to the max by padding 
          // available digits with leading zeroes ["0"].
          local.NewSuffixOfOrderNumber.TextLength16 = "";
          local.CharacterIndex.Count = 1;

          for(var limit = (int)(local.SizeAvailableOnNumber.Count - local
            .LengthOrderNumberSuffix.TotalInteger); local
            .CharacterIndex.Count <= limit; ++local.CharacterIndex.Count)
          {
            local.NewSuffixOfOrderNumber.TextLength16 =
              TrimEnd(local.NewSuffixOfOrderNumber.TextLength16) + "0";
          }

          local.NewSuffixOfOrderNumber.TextLength16 =
            TrimEnd(local.NewSuffixOfOrderNumber.TextLength16) + TrimEnd
            (local.OldSuffixOfOrderNumber.TextLength16);
        }

        // *** Standard Number = County Abbr + Case Year + Case_designator + 
        // Suffix.
        export.LegalAction.StandardNumber =
          TrimEnd(entities.Fips.CountyAbbreviation) + TrimEnd
          (local.CourtOrderYearAaWork.TextValue) + TrimEnd
          (local.CaseType.TextValue) + TrimEnd
          (local.NewSuffixOfOrderNumber.TextLength16);

        break;
      case "OUT OF STATE COURT":
        // The standard number for out of state court cases should be <X><2 
        // digit state abbreviation><9 digit court case number>.
        // (FYI - "Y" and "Z" are also valid for the first character of out of 
        // state court orders, but we only build it
        // as an "X".  The user can change the standard number to "Y" or "Z" if 
        // the "X" results in a duplicate standard number.)
        export.LegalAction.StandardNumber = "X" + entities
          .Fips.StateAbbreviation + Substring
          (import.LegalAction.CourtCaseNumber, 17, 1, 9);

        // 05/15/17  JHarden     CQ 55817   Prevent staff from adding the 
        // standard court order number on LACT.
        if (CharAt(export.LegalAction.StandardNumber, 1) == 'X')
        {
          local.DupFound.Flag = "";

          if (ReadLegalActionTribunal())
          {
            export.LegalAction.StandardNumber =
              entities.Previous.StandardNumber;

            return;
          }
          else
          {
            local.DupFound.Flag = "Y";
          }

          if (AsChar(local.DupFound.Flag) == 'Y')
          {
            local.DupFound.Flag = "";
            local.CompileNumber.StandardNumber = "X" + Substring
              (export.LegalAction.StandardNumber, 20, 2, 19);

            if (ReadLegalAction())
            {
              local.DupFound.Flag = "Y";
            }
          }
          else
          {
            export.LegalAction.StandardNumber =
              local.CompileNumber.StandardNumber ?? "";

            return;
          }

          if (AsChar(local.DupFound.Flag) == 'Y')
          {
            local.DupFound.Flag = "";
            local.CompileNumber.StandardNumber = "Y" + Substring
              (export.LegalAction.StandardNumber, 20, 2, 19);

            if (ReadLegalAction())
            {
              local.DupFound.Flag = "Y";
            }
          }
          else
          {
            export.LegalAction.StandardNumber =
              local.CompileNumber.StandardNumber ?? "";

            return;
          }

          if (AsChar(local.DupFound.Flag) == 'Y')
          {
            local.DupFound.Flag = "";
            local.CompileNumber.StandardNumber = "Z" + Substring
              (export.LegalAction.StandardNumber, 20, 2, 19);

            if (ReadLegalAction())
            {
              local.DupFound.Flag = "Y";
            }
          }
          else
          {
            export.LegalAction.StandardNumber =
              local.CompileNumber.StandardNumber ?? "";

            return;
          }

          if (AsChar(local.DupFound.Flag) == 'Y')
          {
            local.DupFound.Flag = "";
            local.CompileNumber.StandardNumber = "V" + Substring
              (export.LegalAction.StandardNumber, 20, 2, 19);

            if (ReadLegalAction())
            {
              local.DupFound.Flag = "Y";
            }
          }
          else
          {
            export.LegalAction.StandardNumber =
              local.CompileNumber.StandardNumber ?? "";

            return;
          }

          if (AsChar(local.DupFound.Flag) == 'Y')
          {
            local.DupFound.Flag = "";
            local.CompileNumber.StandardNumber = "W" + Substring
              (export.LegalAction.StandardNumber, 20, 2, 19);

            if (ReadLegalAction())
            {
              export.LegalAction.StandardNumber =
                local.CompileNumber.StandardNumber ?? "";
              ExitState = "LE0000_DUPLICATE_STANDARD_NO";

              return;
            }

            export.LegalAction.StandardNumber =
              local.CompileNumber.StandardNumber ?? "";
          }
          else
          {
            export.LegalAction.StandardNumber =
              local.CompileNumber.StandardNumber ?? "";
          }
        }

        break;
      case "TRIBAL COURT":
        // The standard number for Tribal (Indian) tribunals should be the same 
        // as the court case number.
        export.LegalAction.StandardNumber =
          Substring(import.LegalAction.CourtCaseNumber, 1, 12);

        break;
      default:
        break;
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

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.Previous.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.CompileNumber.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Previous.Identifier = db.GetInt32(reader, 0);
        entities.Previous.InitiatingState = db.GetNullableString(reader, 1);
        entities.Previous.InitiatingCounty = db.GetNullableString(reader, 2);
        entities.Previous.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.Previous.StandardNumber = db.GetNullableString(reader, 4);
        entities.Previous.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Previous.TrbId = db.GetNullableInt32(reader, 6);
        entities.Previous.Populated = true;
      });
  }

  private bool ReadLegalActionTribunal()
  {
    entities.ReadTribunal.Populated = false;
    entities.Previous.Populated = false;

    return Read("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.Previous.Identifier = db.GetInt32(reader, 0);
        entities.Previous.InitiatingState = db.GetNullableString(reader, 1);
        entities.Previous.InitiatingCounty = db.GetNullableString(reader, 2);
        entities.Previous.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.Previous.StandardNumber = db.GetNullableString(reader, 4);
        entities.Previous.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Previous.TrbId = db.GetNullableInt32(reader, 6);
        entities.ReadTribunal.Identifier = db.GetInt32(reader, 6);
        entities.ReadTribunal.JudicialDivision =
          db.GetNullableString(reader, 7);
        entities.ReadTribunal.Name = db.GetString(reader, 8);
        entities.ReadTribunal.FipLocation = db.GetNullableInt32(reader, 9);
        entities.ReadTribunal.JudicialDistrict = db.GetString(reader, 10);
        entities.ReadTribunal.FipCounty = db.GetNullableInt32(reader, 11);
        entities.ReadTribunal.FipState = db.GetNullableInt32(reader, 12);
        entities.ReadTribunal.Populated = true;
        entities.Previous.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Tribunal tribunal;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SpTextWorkArea.
    /// </summary>
    [JsonPropertyName("spTextWorkArea")]
    public SpTextWorkArea SpTextWorkArea
    {
      get => spTextWorkArea ??= new();
      set => spTextWorkArea = value;
    }

    /// <summary>
    /// A value of CourtOrderYearCommon.
    /// </summary>
    [JsonPropertyName("courtOrderYearCommon")]
    public Common CourtOrderYearCommon
    {
      get => courtOrderYearCommon ??= new();
      set => courtOrderYearCommon = value;
    }

    /// <summary>
    /// A value of CaseType.
    /// </summary>
    [JsonPropertyName("caseType")]
    public AaWork CaseType
    {
      get => caseType ??= new();
      set => caseType = value;
    }

    /// <summary>
    /// A value of PositionOfCaseType.
    /// </summary>
    [JsonPropertyName("positionOfCaseType")]
    public Common PositionOfCaseType
    {
      get => positionOfCaseType ??= new();
      set => positionOfCaseType = value;
    }

    /// <summary>
    /// A value of CourtOrderYearAaWork.
    /// </summary>
    [JsonPropertyName("courtOrderYearAaWork")]
    public AaWork CourtOrderYearAaWork
    {
      get => courtOrderYearAaWork ??= new();
      set => courtOrderYearAaWork = value;
    }

    /// <summary>
    /// A value of CaseTypeWithSuffix.
    /// </summary>
    [JsonPropertyName("caseTypeWithSuffix")]
    public AaWork CaseTypeWithSuffix
    {
      get => caseTypeWithSuffix ??= new();
      set => caseTypeWithSuffix = value;
    }

    /// <summary>
    /// A value of LengthOfCaseTypeSuffix.
    /// </summary>
    [JsonPropertyName("lengthOfCaseTypeSuffix")]
    public Common LengthOfCaseTypeSuffix
    {
      get => lengthOfCaseTypeSuffix ??= new();
      set => lengthOfCaseTypeSuffix = value;
    }

    /// <summary>
    /// A value of PositionOfOrderSuffix.
    /// </summary>
    [JsonPropertyName("positionOfOrderSuffix")]
    public Common PositionOfOrderSuffix
    {
      get => positionOfOrderSuffix ??= new();
      set => positionOfOrderSuffix = value;
    }

    /// <summary>
    /// A value of OldSuffixOfOrderNumber.
    /// </summary>
    [JsonPropertyName("oldSuffixOfOrderNumber")]
    public AaWork OldSuffixOfOrderNumber
    {
      get => oldSuffixOfOrderNumber ??= new();
      set => oldSuffixOfOrderNumber = value;
    }

    /// <summary>
    /// A value of NewSuffixOfOrderNumber.
    /// </summary>
    [JsonPropertyName("newSuffixOfOrderNumber")]
    public AaWork NewSuffixOfOrderNumber
    {
      get => newSuffixOfOrderNumber ??= new();
      set => newSuffixOfOrderNumber = value;
    }

    /// <summary>
    /// A value of LengthOrderNumberSuffix.
    /// </summary>
    [JsonPropertyName("lengthOrderNumberSuffix")]
    public Common LengthOrderNumberSuffix
    {
      get => lengthOrderNumberSuffix ??= new();
      set => lengthOrderNumberSuffix = value;
    }

    /// <summary>
    /// A value of SizeAvailableOnNumber.
    /// </summary>
    [JsonPropertyName("sizeAvailableOnNumber")]
    public Common SizeAvailableOnNumber
    {
      get => sizeAvailableOnNumber ??= new();
      set => sizeAvailableOnNumber = value;
    }

    /// <summary>
    /// A value of CharacterIndex.
    /// </summary>
    [JsonPropertyName("characterIndex")]
    public Common CharacterIndex
    {
      get => characterIndex ??= new();
      set => characterIndex = value;
    }

    /// <summary>
    /// A value of DupFound.
    /// </summary>
    [JsonPropertyName("dupFound")]
    public Common DupFound
    {
      get => dupFound ??= new();
      set => dupFound = value;
    }

    /// <summary>
    /// A value of CompileNumber.
    /// </summary>
    [JsonPropertyName("compileNumber")]
    public LegalAction CompileNumber
    {
      get => compileNumber ??= new();
      set => compileNumber = value;
    }

    private SpTextWorkArea spTextWorkArea;
    private Common courtOrderYearCommon;
    private AaWork caseType;
    private Common positionOfCaseType;
    private AaWork courtOrderYearAaWork;
    private AaWork caseTypeWithSuffix;
    private Common lengthOfCaseTypeSuffix;
    private Common positionOfOrderSuffix;
    private AaWork oldSuffixOfOrderNumber;
    private AaWork newSuffixOfOrderNumber;
    private Common lengthOrderNumberSuffix;
    private Common sizeAvailableOnNumber;
    private Common characterIndex;
    private Common dupFound;
    private LegalAction compileNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReadTribunal.
    /// </summary>
    [JsonPropertyName("readTribunal")]
    public Tribunal ReadTribunal
    {
      get => readTribunal ??= new();
      set => readTribunal = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private Tribunal readTribunal;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction previous;
  }
#endregion
}
