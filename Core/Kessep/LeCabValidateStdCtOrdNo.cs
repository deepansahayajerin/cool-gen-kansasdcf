// Program: LE_CAB_VALIDATE_STD_CT_ORD_NO, ID: 371985175, model: 746.
// Short name: SWE01941
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
/// A program: LE_CAB_VALIDATE_STD_CT_ORD_NO.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block validates the standard number for US tribunals.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabValidateStdCtOrdNo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_VALIDATE_STD_CT_ORD_NO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabValidateStdCtOrdNo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabValidateStdCtOrdNo.
  /// </summary>
  public LeCabValidateStdCtOrdNo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *** Maintenance Log
    // ---------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  -----------	----------	
    // -----------------------------------------------------------------
    // 01/01/97  govind			Initial Code
    // 10/24/97  govind			Changed the edit to check for numeric only 7-11th.
    // 					12th char can be alpha to allow multiple courts in the same county.
    // 03/10/98  RCG		H00032901 	Add trim function to FIND expression evaluating
    // imported
    // 					standard no for embedded spaces.
    // 05/29/00  JMagat	PR#96185	Remove 7-11th position validation of Standard 
    // Number.
    // 04/02/01  GVandy	PR#108247	The standard number cannot differ from 
    // previous standard numbers
    // 					for the court case.
    // 04/02/01  GVandy	PR#115954	Prevent duplicate standard numbers on multiple
    // court case numbers.
    // 08/20/01  GVandy	WR 10346	Allow 'B' Class standard numbers to be in any 
    // format.
    // 05/06/03  GVandy	PR126879	Restructured and added support for foreign 
    // court orders.
    // 12/07/10  GVandy	CQ23919		Allow prefix "V" and "W" for out of state 
    // standard numbers.
    // ---------------------------------------------------------------------------------------------------------
    // *************************************************************************************
    // *** Purpose:  Ensure STANDARD NUMBER is in expected format:
    // Size: Not to exceed 12 characters.
    // KANSAS: ctyyCDnnnnnn
    //  where	ct = 2-byte alpha county
    // 	yy = 2-byte numeric year
    // 	CD = 2 byte non-blank alpha case designator; 2nd byte may be "*" 
    // asterisk
    // 	nnnnnn = 1-6 suffix
    // Out-of-State: xSSccccccccc
    //  where	x = V, W, X, Y, or Z
    // 	SS = state abbr
    // 	ccccccccc = as is [no validation].
    // Foreign: fCCccccccccc
    //  where	f = F or G
    //  	CC = country abbr
    // 	ccccccccc = as is (no validation).
    // Tribal Court: cccccccccccc
    //  where	cccccccccccc = as is [no validation].
    // *************************************************************************************
    if (!IsEmpty(Substring(import.LegalAction.StandardNumber, 13, 8)))
    {
      // -- The standard number cannot be more than 12 characters in length.
      ExitState = "LE0000_13_THRU_20_MUST_BE_BLANK";

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
        // -- Standard numbers for Bankruptcy legal actions ('B' Class) may be 
        // in any format.
        //    No validation required.  Continue.
        break;
      case "FOREIGN COURT":
        // The standard number for foreign (i.e. non US) court cases should be 
        // <F or G><2 digit country abbreviation><9 digit court case number>.
        // -- Position 1 must be F or G.
        switch(TrimEnd(Substring(import.LegalAction.StandardNumber, 1, 1)))
        {
          case "F":
            break;
          case "G":
            break;
          default:
            ExitState = "LE0000_FOREIGN_STD_NO_1ST_F_OR_G";

            return;
        }

        // -- Positions 2-3 must be the state abbreviation.
        if (!Equal(import.LegalAction.StandardNumber, 2, 2,
          entities.FipsTribAddress.Country))
        {
          ExitState = "LE0000_STDN_2_3_MUST_BE_COUNTRY";

          return;
        }

        break;
      case "KANSAS COURT":
        // The standard number for Kansas court cases should be
        // <2 digit county abbreviation><2 digit year><2 digit case type 
        // designator><6 digit zero padded court case suffix number>.
        // -- The standard number cannot contain embedded spaces.
        if (Find(TrimEnd(import.LegalAction.StandardNumber), " ") != 0)
        {
          ExitState = "LE0000_STDN_BLANKS_NOT_ALLOWED";

          return;
        }

        // -- Positions 1-2 of the standard number must be the county 
        // abrreviation.
        if (!Equal(import.LegalAction.StandardNumber, 1, 2,
          entities.Fips.CountyAbbreviation))
        {
          ExitState = "LE0000_STDNO_1ST_2_CHR_NOT_CNTY";

          return;
        }

        // -- Positions 3-4 of the standard number must be numeric.
        if (Verify(Substring(import.LegalAction.StandardNumber, 20, 3, 2),
          "0123456789") > 0)
        {
          ExitState = "LE0000_STDNO_3_N_4_MUST_BE_NUM";

          return;
        }

        // -- Positions 3-4 of the standard number must match positions 1-2 of 
        // the court case number.
        local.StdNoYear.Count =
          (int)StringToNumber(Substring(
            import.LegalAction.StandardNumber, 20, 3, 2));
        local.CtCaseNoYear.Count =
          (int)StringToNumber(Substring(
            import.LegalAction.CourtCaseNumber, 17, 1, 2));

        if (local.StdNoYear.Count != local.CtCaseNoYear.Count)
        {
          ExitState = "LE0000_STDN_YEAR_DIFF_CT_CASE_YR";

          return;
        }

        // -- Position 5 of the standard number must be alpha.
        if (Verify(Substring(import.LegalAction.StandardNumber, 20, 5, 1),
          "ABCDEFGHIJKLMNOPQRSTUVWXYZ") > 0)
        {
          ExitState = "LE0000_STDNO_5TH_MUST_BE_ALPHA";

          return;
        }

        // -- Position 6 of the standard number must be alpha or *.
        if (Verify(Substring(import.LegalAction.StandardNumber, 20, 6, 1),
          "*ABCDEFGHIJKLMNOPQRSTUVWXYZ") > 0)
        {
          ExitState = "LE0000_STDN_6TH_MUST_AST_OR_ALPH";

          return;
        }

        // ****************************************************************
        // For multi-tribunal counties, the last position of the standard
        // number must match the last position of the court case
        // number.
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
        if (import.Tribunal.Identifier > 0)
        {
          switch(import.Tribunal.Identifier)
          {
            case 669:
              // -- Cowley county - Winfield
              break;
            case 670:
              // -- Cowley county - Arkansas City
              break;
            case 671:
              // -- Crawford county - Girard
              break;
            case 672:
              // -- Crawford county - Pittsburg
              break;
            case 703:
              // -- Labette county - Parsons
              break;
            case 704:
              // -- Labette county - Oswege
              break;
            case 717:
              // -- Montgomery county - Independence
              break;
            case 718:
              // -- Montgomery county - Coffeyville
              break;
            case 722:
              // -- Neosho county - Chanute
              break;
            case 723:
              // -- Neosho county - Erie
              break;
            default:
              goto Test;
          }

          local.MutliTribunalCcNo.Count =
            Length(TrimEnd(import.LegalAction.CourtCaseNumber));
          local.MutliTribunalCcNo.SelectChar =
            Substring(import.LegalAction.CourtCaseNumber,
            local.MutliTribunalCcNo.Count, 1);
          local.MultiTribunalStandardNo.Count =
            Length(TrimEnd(import.LegalAction.StandardNumber));
          local.MultiTribunalStandardNo.SelectChar =
            Substring(import.LegalAction.StandardNumber,
            local.MultiTribunalStandardNo.Count, 1);

          if (AsChar(local.MutliTribunalCcNo.SelectChar) != AsChar
            (local.MultiTribunalStandardNo.SelectChar))
          {
            ExitState = "LE0000_INVALID_MULTI_TRIB_STD_NO";

            return;
          }
        }

        break;
      case "OUT OF STATE COURT":
        // The standard number for out of state court cases should be <X,Y,or 
        // Z><2 digit state abbreviation><9 digit court case number>.
        // -- Position 1 must be V, W, X, Y, or Z.
        switch(TrimEnd(Substring(import.LegalAction.StandardNumber, 1, 1)))
        {
          case "V":
            break;
          case "W":
            break;
          case "X":
            break;
          case "Y":
            break;
          case "Z":
            break;
          default:
            ExitState = "LE0000_STDN_OS_1ST_CHAR_VWXYZ";

            return;
        }

        // -- Positions 2-3 must be be the state abbreviation.
        if (!Equal(import.LegalAction.StandardNumber, 2, 2,
          entities.Fips.StateAbbreviation))
        {
          ExitState = "LE0000_STDN_OS_2_3_BE_STATE";

          return;
        }

        break;
      case "TRIBAL COURT":
        // -- Standard numbers for Tribal courts (fips location codes 20 through
        // 99) may be in any format.
        //    No validation required.  Continue.
        break;
      default:
        break;
    }

Test:

    // *************************************************************************************
    // Verify that the standard number is not used on more than one court case.
    // *************************************************************************************
    foreach(var item in ReadLegalActionTribunal())
    {
      if (AsChar(import.LegalAction.Classification) == 'B' && AsChar
        (entities.LegalAction.Classification) != 'B')
      {
        // -- Only consider standard numbers on B class actions when changing a 
        // B class action.
        continue;
      }

      if (AsChar(import.LegalAction.Classification) != 'B' && AsChar
        (entities.LegalAction.Classification) == 'B')
      {
        // -- Only consider standard numbers on non-B class actions when 
        // changing a non-B class action.
        continue;
      }

      if ((entities.Tribunal.Identifier != import.Tribunal.Identifier || !
        Equal(entities.LegalAction.CourtCaseNumber,
        import.LegalAction.CourtCaseNumber)) && !
        IsEmpty(entities.LegalAction.CourtCaseNumber))
      {
        if (entities.Tribunal.Identifier == import
          .LaccPrevTribunal.Identifier && Equal
          (entities.LegalAction.CourtCaseNumber,
          import.LaccPrevLegalAction.CourtCaseNumber))
        {
          // -- This is OK.  This will happen on LACC when the tribunal or court
          // case number is changed,
          // but the standard number is not changed.
          continue;
        }
        else
        {
          // -- A standard number cannot be duplicated across court case 
          // numbers.
          ExitState = "LE0000_DUPLICATE_STANDARD_NO";

          return;
        }
      }
    }

    // *************************************************************************************
    // Verify that only one standard number is used on this court case.
    // *************************************************************************************
    foreach(var item in ReadLegalAction())
    {
      if (AsChar(import.LegalAction.Classification) == 'B' && AsChar
        (entities.LegalAction.Classification) != 'B')
      {
        // -- Only consider standard numbers on B class actions when changing a 
        // B class action.
        continue;
      }

      if (AsChar(import.LegalAction.Classification) != 'B' && AsChar
        (entities.LegalAction.Classification) == 'B')
      {
        // -- Only consider standard numbers on non-B class actions when 
        // changing a non-B class action.
        continue;
      }

      if (!Equal(entities.LegalAction.StandardNumber,
        import.LegalAction.StandardNumber) && !
        IsEmpty(entities.LegalAction.StandardNumber))
      {
        if (Equal(entities.LegalAction.StandardNumber,
          import.LaccPrevLegalAction.StandardNumber))
        {
          // -- This is OK.  This will happen on LACC when the standard number 
          // is changed
          // but the tribunal and court case number are not changed.
          continue;
        }
        else
        {
          // -- Standard number must be the same for all legal actions with this
          // court case number.
          if (AsChar(import.LegalAction.Classification) == 'B')
          {
            ExitState = "LE0000_STANDARD_NO_DOESNT_MATCH2";
          }
          else
          {
            ExitState = "LE0000_STANDARD_NO_DOESNT_MATCH";
          }

          return;
        }
      }
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

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.LegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 6);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
        entities.Tribunal.Populated = true;

        return true;
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
    /// A value of LaccPrevLegalAction.
    /// </summary>
    [JsonPropertyName("laccPrevLegalAction")]
    public LegalAction LaccPrevLegalAction
    {
      get => laccPrevLegalAction ??= new();
      set => laccPrevLegalAction = value;
    }

    /// <summary>
    /// A value of LaccPrevTribunal.
    /// </summary>
    [JsonPropertyName("laccPrevTribunal")]
    public Tribunal LaccPrevTribunal
    {
      get => laccPrevTribunal ??= new();
      set => laccPrevTribunal = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction laccPrevLegalAction;
    private Tribunal laccPrevTribunal;
    private Tribunal tribunal;
    private LegalAction legalAction;
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
    /// A value of MultiTribunalStandardNo.
    /// </summary>
    [JsonPropertyName("multiTribunalStandardNo")]
    public Common MultiTribunalStandardNo
    {
      get => multiTribunalStandardNo ??= new();
      set => multiTribunalStandardNo = value;
    }

    /// <summary>
    /// A value of MutliTribunalCcNo.
    /// </summary>
    [JsonPropertyName("mutliTribunalCcNo")]
    public Common MutliTribunalCcNo
    {
      get => mutliTribunalCcNo ??= new();
      set => mutliTribunalCcNo = value;
    }

    /// <summary>
    /// A value of CtCaseNoYear.
    /// </summary>
    [JsonPropertyName("ctCaseNoYear")]
    public Common CtCaseNoYear
    {
      get => ctCaseNoYear ??= new();
      set => ctCaseNoYear = value;
    }

    /// <summary>
    /// A value of StdNoYear.
    /// </summary>
    [JsonPropertyName("stdNoYear")]
    public Common StdNoYear
    {
      get => stdNoYear ??= new();
      set => stdNoYear = value;
    }

    /// <summary>
    /// A value of SpTextWorkArea.
    /// </summary>
    [JsonPropertyName("spTextWorkArea")]
    public SpTextWorkArea SpTextWorkArea
    {
      get => spTextWorkArea ??= new();
      set => spTextWorkArea = value;
    }

    private Common multiTribunalStandardNo;
    private Common mutliTribunalCcNo;
    private Common ctCaseNoYear;
    private Common stdNoYear;
    private SpTextWorkArea spTextWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
  }
#endregion
}
