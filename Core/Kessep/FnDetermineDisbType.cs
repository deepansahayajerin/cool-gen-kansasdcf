// Program: FN_DETERMINE_DISB_TYPE, ID: 372544593, model: 746.
// Short name: SWE00434
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DETERMINE_DISB_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will determine the disbursement type for the disbursement.
/// </para>
/// </summary>
[Serializable]
public partial class FnDetermineDisbType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_DISB_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineDisbType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineDisbType.
  /// </summary>
  public FnDetermineDisbType(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************
    // Date	Developer	Description
    // 080997	T.O.Redmond	Fix NA ACS code
    // 030498  R.B.Mohapatra   Included all the Interstate Disb_type Codes
    // 01/17/99     RK     Changed and added codes.
    // 09/23/99 PR# H74065 SWSRKXD
    // Add set statements for new VOL disb types - FC VOL, NC VOL, NF VOL and 
    // FCI VOL.
    // 10/19/99 SWSRKXD
    // This PR has been backed out per SMEs request.
    // 10/19/99 PR# H77874 SWSRKXD
    // Money will never be disbursed for "NC" programs. B650 will
    // NEVER create any disb credit for NC.
    // 02-15-00  PR 86861  Fangman  CR Fee changes and restructuring.
    // 10-05-00  PR 98039  Fangman  Added attrib to imp per view to match pstep.
    // ********************************************
    // ---------------------------------------------
    // Date	  By	  IDCR#	 Description
    // 08/20/03  Fangman  302055   Changed view of imp_per colleciton to no 
    // longer be exported.
    // ---------------------------------------------
    UseFnHardcodedCashReceipting();
    local.Af.ProgramAppliedTo = "AF";
    local.Fc.ProgramAppliedTo = "FC";
    local.Nf.ProgramAppliedTo = "NF";
    local.Na.ProgramAppliedTo = "NA";
    local.Nc.ProgramAppliedTo = "NC";
    local.Afi.ProgramAppliedTo = "AFI";
    local.Fci.ProgramAppliedTo = "FCI";
    local.Nai.ProgramAppliedTo = "NAI";
    UseFnHardcodedDebtDistribution();
    local.GiftObligationType.SystemGeneratedIdentifier = 22;
    UseFnHardcodedDisbursementInfo();

    // ***************************************************************
    // Changes made:
    // Spousal: NAI ASP to NAI ISP(on interest one only)
    // Added 'i's to Medical Cost group in AFI,FCI and NAI.
    // Arreage Judgement goes from  ARRJ to AAJ, IARJ to IAJ
    // Spousal Arrears goes from ASP and ISP to ASAJ and ISAJ.
    // Medical Judgement goes from AMS, IMS to AMJ & IMJ.
    // Interest Judgement goes from INTJ to IIJ.
    // Cost of Raising Child goes from CRC, ICRC to ACRCH and ICRCH.
    // ****************************************************************
    // ****************************************************************
    // Latest changes: To handle the new gift codes. 3/10/99
    // 28 new codes were added to handle the new G (gift) applied to code. 8 in 
    // CS, 4 in spousal, 8 in medical, 8 in medical cost
    // ****************************************************************
    // *****  Child Support - Group 1
    if (import.ObligationType.SystemGeneratedIdentifier == local
      .Cs.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfGcs.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcGcs.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaGcs.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfGcs.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiGcs.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciGcs.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiCcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIcs.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiGcs.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Spousal Support - Group 2
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Sp.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfCsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfGsp.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaCsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaGsp.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiCsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiGsp.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiCsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIsp.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiGsp.SystemGeneratedIdentifier;
          }

          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Medical Support - Group 3
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Ms.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfGms.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiGms.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcGms.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciGms.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaGms.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiGms.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfCms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfAms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfIms.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfGms.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Medical Cost - Group 4
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Mc.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfGmc.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiGmc.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaGmc.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiGmc.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcGmc.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciGmc.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToCurrent.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfCmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfAmc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfImc.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToGift.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfGmc.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Arrearrage Judgement - Group 5
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Arrj.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIaj.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIaj.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcIaj.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciIaj.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIaj.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIaj.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfAaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfIaj.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Spousal Arrears Judgement - Group 6
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Saj.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAsaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIsaj.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAsaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIsaj.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAsaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIsaj.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAsaj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIsaj.SystemGeneratedIdentifier;
          }

          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Medical Judgement - Group 7
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Mj.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfImj.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiImj.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcImj.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciImj.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaImj.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiImj.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfAmj.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfImj.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Percent Uninsured Medical Expense Judgement - Group 8
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Pume.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfUme.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIume1.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiUme2.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIume.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaUme.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIume1.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiUme2.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIume.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcUme.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcIume1.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciUme2.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciIume.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfUme.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfIume1.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }

      // *****  Interest Judgement - Group 9
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Intj.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.AfIij.SystemGeneratedIdentifier;

          break;
        case "AFI":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.AfiIij.SystemGeneratedIdentifier;

          break;
        case "FC":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.FcIij.SystemGeneratedIdentifier;

          break;
        case "FCI":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.FciIij.SystemGeneratedIdentifier;

          break;
        case "NA":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.NaIij.SystemGeneratedIdentifier;

          break;
        case "NAI":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.NaiIij.SystemGeneratedIdentifier;

          break;
        case "NF":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.NfIij.SystemGeneratedIdentifier;

          break;
        case "NC":
          break;
        default:
          break;
      }

      // *****  Cost of Raising Child - Group 10
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Crch.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "AFI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.AfiIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "FC":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FcIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "FCI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.FciIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "NA":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "NAI":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NaiIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "NF":
          if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToArrears.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfAcrch.SystemGeneratedIdentifier;
          }
          else if (AsChar(import.Per.AppliedToCode) == AsChar
            (local.AppliedToInterest.AppliedToCode))
          {
            export.DisbursementType.SystemGeneratedIdentifier =
              local.NfIcrch.SystemGeneratedIdentifier;
          }

          break;
        case "NC":
          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .IvdRcObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.IvdRcDisbursementType.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .IrsNegObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.IrsNegDisbursementType.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .BdckRcObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.BdckRcDisbursementType.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .MisArObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.MisArDisbursementType.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .MisApObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.MisApDisbursementType.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .MisNonObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.MisNonDisbursementType.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .Vol.SystemGeneratedIdentifier)
    {
      switch(TrimEnd(import.Per.ProgramAppliedTo))
      {
        case "AF":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.AfVol.SystemGeneratedIdentifier;

          break;
        case "AFI":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.AfiVol.SystemGeneratedIdentifier;

          break;
        case "NA":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.NaVol.SystemGeneratedIdentifier;

          break;
        case "NAI":
          export.DisbursementType.SystemGeneratedIdentifier =
            local.NaiVol.SystemGeneratedIdentifier;

          break;
        default:
          ExitState = "FN0000_INVALID_PRGM_APPLD_2_CD";

          break;
      }
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .ApFees.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.ApFee.SystemGeneratedIdentifier;
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .N718bbligationType.SystemGeneratedIdentifier)
    {
      if (AsChar(import.Per.AppliedToCode) == AsChar
        (local.AppliedToArrears.AppliedToCode))
      {
        export.DisbursementType.SystemGeneratedIdentifier =
          local.N718bisbursementType.SystemGeneratedIdentifier;
      }
      else if (AsChar(import.Per.AppliedToCode) == AsChar
        (local.AppliedToInterest.AppliedToCode))
      {
        export.DisbursementType.SystemGeneratedIdentifier =
          local.I718b.SystemGeneratedIdentifier;
      }
    }
    else if (import.ObligationType.SystemGeneratedIdentifier == local
      .CrFeeObligationType.SystemGeneratedIdentifier)
    {
      export.DisbursementType.SystemGeneratedIdentifier =
        local.CrFeeDisbursementType.SystemGeneratedIdentifier;
    }
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Cash.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.Cs.SystemGeneratedIdentifier =
      useExport.OtChildSupport.SystemGeneratedIdentifier;
    local.Sp.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
    local.Ms.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.Mc.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
    local.Arrj.SystemGeneratedIdentifier =
      useExport.OtArrearsJudgement.SystemGeneratedIdentifier;
    local.Saj.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.Mj.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
    local.Pume.SystemGeneratedIdentifier =
      useExport.OtPctUnisuredMedExpJudgemnt.SystemGeneratedIdentifier;
    local.Intj.SystemGeneratedIdentifier =
      useExport.OtInterestJudgement.SystemGeneratedIdentifier;
    local.Crch.SystemGeneratedIdentifier =
      useExport.OtCostOfRasingChild.SystemGeneratedIdentifier;
    local.IvdRcObligationType.SystemGeneratedIdentifier =
      useExport.OtIvdRecovery.SystemGeneratedIdentifier;
    local.IrsNegObligationType.SystemGeneratedIdentifier =
      useExport.OtIrsNegative.SystemGeneratedIdentifier;
    local.BdckRcObligationType.SystemGeneratedIdentifier =
      useExport.OtBadCheck.SystemGeneratedIdentifier;
    local.MisArObligationType.SystemGeneratedIdentifier =
      useExport.OtArMisdirectedPymnt.SystemGeneratedIdentifier;
    local.MisApObligationType.SystemGeneratedIdentifier =
      useExport.OtApMisdirectedPymnt.SystemGeneratedIdentifier;
    local.MisNonObligationType.SystemGeneratedIdentifier =
      useExport.OtNonCsePrnMisdirectedPymnt.SystemGeneratedIdentifier;
    local.Vol.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.ApFees.SystemGeneratedIdentifier =
      useExport.OtApFees.SystemGeneratedIdentifier;
    local.N718bbligationType.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.AppliedToCurrent.AppliedToCode = useExport.CollCurrent.AppliedToCode;
    local.AppliedToArrears.AppliedToCode = useExport.CollArrears.AppliedToCode;
    local.AppliedToInterest.AppliedToCode =
      useExport.CollInterest.AppliedToCode;
    local.AppliedToGift.AppliedToCode = useExport.CollGift.AppliedToCode;
  }

  private void UseFnHardcodedDisbursementInfo()
  {
    var useImport = new FnHardcodedDisbursementInfo.Import();
    var useExport = new FnHardcodedDisbursementInfo.Export();

    Call(FnHardcodedDisbursementInfo.Execute, useImport, useExport);

    local.AfIms.SystemGeneratedIdentifier =
      useExport.AfIms.SystemGeneratedIdentifier;
    local.AfIsp.SystemGeneratedIdentifier =
      useExport.AfIsp.SystemGeneratedIdentifier;
    local.AfAsp.SystemGeneratedIdentifier =
      useExport.AfAsp.SystemGeneratedIdentifier;
    local.AfAms.SystemGeneratedIdentifier =
      useExport.AfAms.SystemGeneratedIdentifier;
    local.AfCcs.SystemGeneratedIdentifier =
      useExport.AfCcs.SystemGeneratedIdentifier;
    local.AfAcs.SystemGeneratedIdentifier =
      useExport.AfAcs.SystemGeneratedIdentifier;
    local.AfIcs.SystemGeneratedIdentifier =
      useExport.AfIcs.SystemGeneratedIdentifier;
    local.AfCsp.SystemGeneratedIdentifier =
      useExport.AfCsp.SystemGeneratedIdentifier;
    local.AfCms.SystemGeneratedIdentifier =
      useExport.AfCms.SystemGeneratedIdentifier;
    local.AfiCms.SystemGeneratedIdentifier =
      useExport.AfiCms.SystemGeneratedIdentifier;
    local.AfCmc.SystemGeneratedIdentifier =
      useExport.AfCmc.SystemGeneratedIdentifier;
    local.AfAmc.SystemGeneratedIdentifier =
      useExport.AfAmc.SystemGeneratedIdentifier;
    local.AfImc.SystemGeneratedIdentifier =
      useExport.AfImc.SystemGeneratedIdentifier;
    local.AfUme.SystemGeneratedIdentifier =
      useExport.AfUme.SystemGeneratedIdentifier;
    local.AfIume1.SystemGeneratedIdentifier =
      useExport.AfIume1.SystemGeneratedIdentifier;
    local.AfVol.SystemGeneratedIdentifier =
      useExport.AfVol.SystemGeneratedIdentifier;
    local.AfiCcs.SystemGeneratedIdentifier =
      useExport.AfiCcs.SystemGeneratedIdentifier;
    local.AfiAcs.SystemGeneratedIdentifier =
      useExport.AfiAcs.SystemGeneratedIdentifier;
    local.AfiIcs.SystemGeneratedIdentifier =
      useExport.AfiIcs.SystemGeneratedIdentifier;
    local.AfiCsp.SystemGeneratedIdentifier =
      useExport.AfiCsp.SystemGeneratedIdentifier;
    local.AfiIms.SystemGeneratedIdentifier =
      useExport.AfiIms.SystemGeneratedIdentifier;
    local.AfiAms.SystemGeneratedIdentifier =
      useExport.AfiAms.SystemGeneratedIdentifier;
    local.AfiIsp.SystemGeneratedIdentifier =
      useExport.AfiIsp.SystemGeneratedIdentifier;
    local.AfiAsp.SystemGeneratedIdentifier =
      useExport.AfiAsp.SystemGeneratedIdentifier;
    local.AfiAmj.SystemGeneratedIdentifier =
      useExport.AfiAmj.SystemGeneratedIdentifier;
    local.AfiCmc.SystemGeneratedIdentifier =
      useExport.AfiCmc.SystemGeneratedIdentifier;
    local.AfiImc.SystemGeneratedIdentifier =
      useExport.AfiImc.SystemGeneratedIdentifier;
    local.AfiUme2.SystemGeneratedIdentifier =
      useExport.AfiUme2.SystemGeneratedIdentifier;
    local.AfiIume.SystemGeneratedIdentifier =
      useExport.AfiIume.SystemGeneratedIdentifier;
    local.AfiVol.SystemGeneratedIdentifier =
      useExport.AfiVol.SystemGeneratedIdentifier;
    local.FcCcs.SystemGeneratedIdentifier =
      useExport.FcCcs.SystemGeneratedIdentifier;
    local.FcAcs.SystemGeneratedIdentifier =
      useExport.FcAcs.SystemGeneratedIdentifier;
    local.FcIcs.SystemGeneratedIdentifier =
      useExport.FcIcs.SystemGeneratedIdentifier;
    local.FcIms.SystemGeneratedIdentifier =
      useExport.FcIms.SystemGeneratedIdentifier;
    local.FcAms.SystemGeneratedIdentifier =
      useExport.FcAms.SystemGeneratedIdentifier;
    local.FcCms.SystemGeneratedIdentifier =
      useExport.FcCms.SystemGeneratedIdentifier;
    local.FcCmc.SystemGeneratedIdentifier =
      useExport.FcCmc.SystemGeneratedIdentifier;
    local.FcAmc.SystemGeneratedIdentifier =
      useExport.FcAmc.SystemGeneratedIdentifier;
    local.FcImc.SystemGeneratedIdentifier =
      useExport.FcImc.SystemGeneratedIdentifier;
    local.FcUme.SystemGeneratedIdentifier =
      useExport.FcUme.SystemGeneratedIdentifier;
    local.FcIume1.SystemGeneratedIdentifier =
      useExport.FcIume1.SystemGeneratedIdentifier;
    local.FciCcs.SystemGeneratedIdentifier =
      useExport.FciCcs.SystemGeneratedIdentifier;
    local.FciAcs.SystemGeneratedIdentifier =
      useExport.FciAcs.SystemGeneratedIdentifier;
    local.FciIcs.SystemGeneratedIdentifier =
      useExport.FciIcs.SystemGeneratedIdentifier;
    local.FciCms.SystemGeneratedIdentifier =
      useExport.FciCms.SystemGeneratedIdentifier;
    local.FciCmc.SystemGeneratedIdentifier =
      useExport.FciCmc.SystemGeneratedIdentifier;
    local.FciIms.SystemGeneratedIdentifier =
      useExport.FciIms.SystemGeneratedIdentifier;
    local.FciAms.SystemGeneratedIdentifier =
      useExport.FciAms.SystemGeneratedIdentifier;
    local.FciAmc.SystemGeneratedIdentifier =
      useExport.FciAmc.SystemGeneratedIdentifier;
    local.FciImc.SystemGeneratedIdentifier =
      useExport.FciImc.SystemGeneratedIdentifier;
    local.FciUme2.SystemGeneratedIdentifier =
      useExport.FciUme2.SystemGeneratedIdentifier;
    local.FciIume.SystemGeneratedIdentifier =
      useExport.FciIume.SystemGeneratedIdentifier;
    local.NaCcs.SystemGeneratedIdentifier =
      useExport.NaCcs.SystemGeneratedIdentifier;
    local.NaAcs.SystemGeneratedIdentifier =
      useExport.NaAcs.SystemGeneratedIdentifier;
    local.NaIcs.SystemGeneratedIdentifier =
      useExport.NaIcs.SystemGeneratedIdentifier;
    local.NaCsp.SystemGeneratedIdentifier =
      useExport.NaCsp.SystemGeneratedIdentifier;
    local.NaCms.SystemGeneratedIdentifier =
      useExport.NaCms.SystemGeneratedIdentifier;
    local.NaCmc.SystemGeneratedIdentifier =
      useExport.NaCmc.SystemGeneratedIdentifier;
    local.NaAmc.SystemGeneratedIdentifier =
      useExport.NaAmc.SystemGeneratedIdentifier;
    local.NaImc.SystemGeneratedIdentifier =
      useExport.NaImc.SystemGeneratedIdentifier;
    local.NaIsp.SystemGeneratedIdentifier =
      useExport.NaIsp.SystemGeneratedIdentifier;
    local.NaAsp.SystemGeneratedIdentifier =
      useExport.NaAsp.SystemGeneratedIdentifier;
    local.NaArrjr.SystemGeneratedIdentifier =
      useExport.NaArrjr.SystemGeneratedIdentifier;
    local.NaUme.SystemGeneratedIdentifier =
      useExport.NaUme.SystemGeneratedIdentifier;
    local.NaIume1.SystemGeneratedIdentifier =
      useExport.NaIume1.SystemGeneratedIdentifier;
    local.NaIms.SystemGeneratedIdentifier =
      useExport.NaIms.SystemGeneratedIdentifier;
    local.NaAms.SystemGeneratedIdentifier =
      useExport.NaAms.SystemGeneratedIdentifier;
    local.NaVol.SystemGeneratedIdentifier =
      useExport.NaVol.SystemGeneratedIdentifier;
    local.NaiCcs.SystemGeneratedIdentifier =
      useExport.NaiCcs.SystemGeneratedIdentifier;
    local.NaiAcs.SystemGeneratedIdentifier =
      useExport.NaiAcs.SystemGeneratedIdentifier;
    local.NaiIms.SystemGeneratedIdentifier =
      useExport.NaiIms.SystemGeneratedIdentifier;
    local.NaiAms.SystemGeneratedIdentifier =
      useExport.NaiAms.SystemGeneratedIdentifier;
    local.NaiIsp.SystemGeneratedIdentifier =
      useExport.NaiIsp.SystemGeneratedIdentifier;
    local.NaiAsp.SystemGeneratedIdentifier =
      useExport.NaiAsp.SystemGeneratedIdentifier;
    local.NaiIcs.SystemGeneratedIdentifier =
      useExport.NaiIcs.SystemGeneratedIdentifier;
    local.NaiCsp.SystemGeneratedIdentifier =
      useExport.NaiCsp.SystemGeneratedIdentifier;
    local.NaiCms.SystemGeneratedIdentifier =
      useExport.NaiCms.SystemGeneratedIdentifier;
    local.NaiCmc.SystemGeneratedIdentifier =
      useExport.NaiCmc.SystemGeneratedIdentifier;
    local.NaiAmc.SystemGeneratedIdentifier =
      useExport.NaiAmc.SystemGeneratedIdentifier;
    local.NaiImc.SystemGeneratedIdentifier =
      useExport.NaiImc.SystemGeneratedIdentifier;
    local.NaiUme2.SystemGeneratedIdentifier =
      useExport.NaiUme2.SystemGeneratedIdentifier;
    local.NaiIume.SystemGeneratedIdentifier =
      useExport.NaiIume.SystemGeneratedIdentifier;
    local.NaiVol.SystemGeneratedIdentifier =
      useExport.NaiVol.SystemGeneratedIdentifier;
    local.NfCms.SystemGeneratedIdentifier =
      useExport.NfCms.SystemGeneratedIdentifier;
    local.NfCmc.SystemGeneratedIdentifier =
      useExport.NfCmc.SystemGeneratedIdentifier;
    local.NfAmc.SystemGeneratedIdentifier =
      useExport.NfAmc.SystemGeneratedIdentifier;
    local.NfImc.SystemGeneratedIdentifier =
      useExport.NfImc.SystemGeneratedIdentifier;
    local.NfUme.SystemGeneratedIdentifier =
      useExport.NfUme.SystemGeneratedIdentifier;
    local.NfIume1.SystemGeneratedIdentifier =
      useExport.NfIume1.SystemGeneratedIdentifier;
    local.NfCcs.SystemGeneratedIdentifier =
      useExport.NfCcs.SystemGeneratedIdentifier;
    local.NfIms.SystemGeneratedIdentifier =
      useExport.NfIms.SystemGeneratedIdentifier;
    local.NfAms.SystemGeneratedIdentifier =
      useExport.NfAms.SystemGeneratedIdentifier;
    local.NfAcs.SystemGeneratedIdentifier =
      useExport.NfAcs.SystemGeneratedIdentifier;
    local.NfIcs.SystemGeneratedIdentifier =
      useExport.NfIcs.SystemGeneratedIdentifier;
    local.NfiCms.SystemGeneratedIdentifier =
      useExport.NfiCms.SystemGeneratedIdentifier;
    local.NfiAms.SystemGeneratedIdentifier =
      useExport.NfiAms.SystemGeneratedIdentifier;
    local.NfiIms.SystemGeneratedIdentifier =
      useExport.NfiIms.SystemGeneratedIdentifier;
    local.NfiCmc.SystemGeneratedIdentifier =
      useExport.NfiCmc.SystemGeneratedIdentifier;
    local.NfiAmc.SystemGeneratedIdentifier =
      useExport.NfiAmc.SystemGeneratedIdentifier;
    local.NfiImc.SystemGeneratedIdentifier =
      useExport.NfiImc.SystemGeneratedIdentifier;
    local.NfiArrj.SystemGeneratedIdentifier =
      useExport.NfiArrj.SystemGeneratedIdentifier;
    local.NfiIarj.SystemGeneratedIdentifier =
      useExport.NfiIarj.SystemGeneratedIdentifier;
    local.GiftDisbursementType.SystemGeneratedIdentifier =
      useExport.Gift.SystemGeneratedIdentifier;
    local.IvdRcDisbursementType.SystemGeneratedIdentifier =
      useExport.IvdRc.SystemGeneratedIdentifier;
    local.IrsNegDisbursementType.SystemGeneratedIdentifier =
      useExport.IrsNeg.SystemGeneratedIdentifier;
    local.BdckRcDisbursementType.SystemGeneratedIdentifier =
      useExport.BdckRc.SystemGeneratedIdentifier;
    local.MisArDisbursementType.SystemGeneratedIdentifier =
      useExport.MisAr.SystemGeneratedIdentifier;
    local.MisApDisbursementType.SystemGeneratedIdentifier =
      useExport.MisAp.SystemGeneratedIdentifier;
    local.MisNonDisbursementType.SystemGeneratedIdentifier =
      useExport.MisNon.SystemGeneratedIdentifier;
    local.ApFee.SystemGeneratedIdentifier =
      useExport.ApFee.SystemGeneratedIdentifier;
    local.N718bisbursementType.SystemGeneratedIdentifier =
      useExport.N718b.SystemGeneratedIdentifier;
    local.I718b.SystemGeneratedIdentifier =
      useExport.I718b.SystemGeneratedIdentifier;
    local.CrFeeDisbursementType.SystemGeneratedIdentifier =
      useExport.CrFee.SystemGeneratedIdentifier;
    local.AfAaj.SystemGeneratedIdentifier =
      useExport.AfAaj.SystemGeneratedIdentifier;
    local.AfAcrch.SystemGeneratedIdentifier =
      useExport.AfAcrch.SystemGeneratedIdentifier;
    local.AfAmj.SystemGeneratedIdentifier =
      useExport.AfAmj.SystemGeneratedIdentifier;
    local.AfAsaj.SystemGeneratedIdentifier =
      useExport.AfAsaj.SystemGeneratedIdentifier;
    local.AfGcs.SystemGeneratedIdentifier =
      useExport.AfGcs.SystemGeneratedIdentifier;
    local.AfGmc.SystemGeneratedIdentifier =
      useExport.AfGmc.SystemGeneratedIdentifier;
    local.AfGms.SystemGeneratedIdentifier =
      useExport.AfGms.SystemGeneratedIdentifier;
    local.AfGsp.SystemGeneratedIdentifier =
      useExport.AfGsp.SystemGeneratedIdentifier;
    local.AfIaj.SystemGeneratedIdentifier =
      useExport.AfIaj.SystemGeneratedIdentifier;
    local.AfIcrch.SystemGeneratedIdentifier =
      useExport.AfIcrch.SystemGeneratedIdentifier;
    local.AfIij.SystemGeneratedIdentifier =
      useExport.AfIij.SystemGeneratedIdentifier;
    local.AfImj.SystemGeneratedIdentifier =
      useExport.AfImj.SystemGeneratedIdentifier;
    local.AfIsaj.SystemGeneratedIdentifier =
      useExport.AfIsaj.SystemGeneratedIdentifier;
    local.AfiAaj.SystemGeneratedIdentifier =
      useExport.AfiAaj.SystemGeneratedIdentifier;
    local.AfiAcrch.SystemGeneratedIdentifier =
      useExport.AfiAcrch.SystemGeneratedIdentifier;
    local.AfiAmc.SystemGeneratedIdentifier =
      useExport.AfiAmc.SystemGeneratedIdentifier;
    local.AfiAsaj.SystemGeneratedIdentifier =
      useExport.AfiAsaj.SystemGeneratedIdentifier;
    local.AfiGcs.SystemGeneratedIdentifier =
      useExport.AfiGcs.SystemGeneratedIdentifier;
    local.AfiGmc.SystemGeneratedIdentifier =
      useExport.AfiGmc.SystemGeneratedIdentifier;
    local.AfiGms.SystemGeneratedIdentifier =
      useExport.AfiGms.SystemGeneratedIdentifier;
    local.AfiGsp.SystemGeneratedIdentifier =
      useExport.AfiGsp.SystemGeneratedIdentifier;
    local.AfiIaj.SystemGeneratedIdentifier =
      useExport.AfiIaj.SystemGeneratedIdentifier;
    local.AfiIij.SystemGeneratedIdentifier =
      useExport.AfiIij.SystemGeneratedIdentifier;
    local.AfiIcrch.SystemGeneratedIdentifier =
      useExport.AfiIcrch.SystemGeneratedIdentifier;
    local.AfiImj.SystemGeneratedIdentifier =
      useExport.AfiImj.SystemGeneratedIdentifier;
    local.AfiIsaj.SystemGeneratedIdentifier =
      useExport.AfiIsaj.SystemGeneratedIdentifier;
    local.FcAaj.SystemGeneratedIdentifier =
      useExport.FcAaj.SystemGeneratedIdentifier;
    local.FcAmj.SystemGeneratedIdentifier =
      useExport.FcAmj.SystemGeneratedIdentifier;
    local.FcAcrch.SystemGeneratedIdentifier =
      useExport.FcAcrch.SystemGeneratedIdentifier;
    local.FcImj.SystemGeneratedIdentifier =
      useExport.FcImj.SystemGeneratedIdentifier;
    local.FcIcrch.SystemGeneratedIdentifier =
      useExport.FcIcrch.SystemGeneratedIdentifier;
    local.FcGmc.SystemGeneratedIdentifier =
      useExport.FcGmc.SystemGeneratedIdentifier;
    local.FcGms.SystemGeneratedIdentifier =
      useExport.FcGms.SystemGeneratedIdentifier;
    local.FcGcs.SystemGeneratedIdentifier =
      useExport.FcGcs.SystemGeneratedIdentifier;
    local.FcIaj.SystemGeneratedIdentifier =
      useExport.FcIaj.SystemGeneratedIdentifier;
    local.FcIij.SystemGeneratedIdentifier =
      useExport.FcIij.SystemGeneratedIdentifier;
    local.FciAaj.SystemGeneratedIdentifier =
      useExport.FciAaj.SystemGeneratedIdentifier;
    local.FciAmj.SystemGeneratedIdentifier =
      useExport.FciAmj.SystemGeneratedIdentifier;
    local.FciAcrch.SystemGeneratedIdentifier =
      useExport.FciAcrch.SystemGeneratedIdentifier;
    local.FciGmc.SystemGeneratedIdentifier =
      useExport.FciGmc.SystemGeneratedIdentifier;
    local.FciGcs.SystemGeneratedIdentifier =
      useExport.FciGcs.SystemGeneratedIdentifier;
    local.FciGms.SystemGeneratedIdentifier =
      useExport.FciGms.SystemGeneratedIdentifier;
    local.FciIcrch.SystemGeneratedIdentifier =
      useExport.FciIcrch.SystemGeneratedIdentifier;
    local.FciImj.SystemGeneratedIdentifier =
      useExport.FciImj.SystemGeneratedIdentifier;
    local.FciIaj.SystemGeneratedIdentifier =
      useExport.FciIaj.SystemGeneratedIdentifier;
    local.FciIij.SystemGeneratedIdentifier =
      useExport.FciIij.SystemGeneratedIdentifier;
    local.NaGmc.SystemGeneratedIdentifier =
      useExport.NaGmc.SystemGeneratedIdentifier;
    local.NaGms.SystemGeneratedIdentifier =
      useExport.NaGms.SystemGeneratedIdentifier;
    local.NaGsp.SystemGeneratedIdentifier =
      useExport.NaGsp.SystemGeneratedIdentifier;
    local.NaGcs.SystemGeneratedIdentifier =
      useExport.NaGcs.SystemGeneratedIdentifier;
    local.NaIsaj.SystemGeneratedIdentifier =
      useExport.NaIsaj.SystemGeneratedIdentifier;
    local.NaAsaj.SystemGeneratedIdentifier =
      useExport.NaAsaj.SystemGeneratedIdentifier;
    local.NaIcrch.SystemGeneratedIdentifier =
      useExport.NaIcrch1.SystemGeneratedIdentifier;
    local.NaAcrch.SystemGeneratedIdentifier =
      useExport.NaAcrch.SystemGeneratedIdentifier;
    local.NaImj.SystemGeneratedIdentifier =
      useExport.NaImj.SystemGeneratedIdentifier;
    local.NaAmj.SystemGeneratedIdentifier =
      useExport.NaAmj.SystemGeneratedIdentifier;
    local.NaAaj.SystemGeneratedIdentifier =
      useExport.NaAaj.SystemGeneratedIdentifier;
    local.NaIaj.SystemGeneratedIdentifier =
      useExport.NaIaj.SystemGeneratedIdentifier;
    local.NaIij.SystemGeneratedIdentifier =
      useExport.NaIij.SystemGeneratedIdentifier;
    local.NaiIsaj.SystemGeneratedIdentifier =
      useExport.NaiIsaj.SystemGeneratedIdentifier;
    local.NaiGmc.SystemGeneratedIdentifier =
      useExport.NaiGmc.SystemGeneratedIdentifier;
    local.NaiGms.SystemGeneratedIdentifier =
      useExport.NaiGms.SystemGeneratedIdentifier;
    local.NaiGsp.SystemGeneratedIdentifier =
      useExport.NaiGsp.SystemGeneratedIdentifier;
    local.NaiGcs.SystemGeneratedIdentifier =
      useExport.NaiGcs.SystemGeneratedIdentifier;
    local.NaiAsaj.SystemGeneratedIdentifier =
      useExport.NaiAsaj.SystemGeneratedIdentifier;
    local.NaiIcrch.SystemGeneratedIdentifier =
      useExport.NaiIcrch.SystemGeneratedIdentifier;
    local.NaiAcrch.SystemGeneratedIdentifier =
      useExport.NaiAcrch.SystemGeneratedIdentifier;
    local.NaiImj.SystemGeneratedIdentifier =
      useExport.NaiImj.SystemGeneratedIdentifier;
    local.NaiAmj.SystemGeneratedIdentifier =
      useExport.NaiAmj.SystemGeneratedIdentifier;
    local.NaiAaj.SystemGeneratedIdentifier =
      useExport.NaiAaj.SystemGeneratedIdentifier;
    local.NaiIaj.SystemGeneratedIdentifier =
      useExport.NaiIaj.SystemGeneratedIdentifier;
    local.NaiIij.SystemGeneratedIdentifier =
      useExport.NaiIij.SystemGeneratedIdentifier;
    local.NcIaj.SystemGeneratedIdentifier =
      useExport.NcIaj.SystemGeneratedIdentifier;
    local.NcIcrch.SystemGeneratedIdentifier =
      useExport.NcIcrch.SystemGeneratedIdentifier;
    local.NcAcrch.SystemGeneratedIdentifier =
      useExport.NcAcrch.SystemGeneratedIdentifier;
    local.NcIij.SystemGeneratedIdentifier =
      useExport.NcIij.SystemGeneratedIdentifier;
    local.NcIume.SystemGeneratedIdentifier =
      useExport.NcIume.SystemGeneratedIdentifier;
    local.NcUme.SystemGeneratedIdentifier =
      useExport.NcUme.SystemGeneratedIdentifier;
    local.NcImj.SystemGeneratedIdentifier =
      useExport.NcImj.SystemGeneratedIdentifier;
    local.NcGmc.SystemGeneratedIdentifier =
      useExport.NcGmc.SystemGeneratedIdentifier;
    local.NcGms.SystemGeneratedIdentifier =
      useExport.NcGms.SystemGeneratedIdentifier;
    local.NcGcs.SystemGeneratedIdentifier =
      useExport.NcGcs.SystemGeneratedIdentifier;
    local.NcAmj.SystemGeneratedIdentifier =
      useExport.NcAmj.SystemGeneratedIdentifier;
    local.NcAaj.SystemGeneratedIdentifier =
      useExport.NcAaj.SystemGeneratedIdentifier;
    local.NcImc.SystemGeneratedIdentifier =
      useExport.NcImc.SystemGeneratedIdentifier;
    local.NcAmc.SystemGeneratedIdentifier =
      useExport.NcAmc.SystemGeneratedIdentifier;
    local.NcCmc.SystemGeneratedIdentifier =
      useExport.NcCmc.SystemGeneratedIdentifier;
    local.NcIms.SystemGeneratedIdentifier =
      useExport.NcIms.SystemGeneratedIdentifier;
    local.NcAms.SystemGeneratedIdentifier =
      useExport.NcAms.SystemGeneratedIdentifier;
    local.NcCms.SystemGeneratedIdentifier =
      useExport.NcCms.SystemGeneratedIdentifier;
    local.NcIcs.SystemGeneratedIdentifier =
      useExport.NcIcs.SystemGeneratedIdentifier;
    local.NcAcs.SystemGeneratedIdentifier =
      useExport.NcAcs.SystemGeneratedIdentifier;
    local.NcCcs.SystemGeneratedIdentifier =
      useExport.NcCcs.SystemGeneratedIdentifier;
    local.NfAaj.SystemGeneratedIdentifier =
      useExport.NfAaj.SystemGeneratedIdentifier;
    local.NfIaj.SystemGeneratedIdentifier =
      useExport.NfIaj.SystemGeneratedIdentifier;
    local.NfIij.SystemGeneratedIdentifier =
      useExport.NfIij.SystemGeneratedIdentifier;
    local.NfGmc.SystemGeneratedIdentifier =
      useExport.NfGmc.SystemGeneratedIdentifier;
    local.NfGms.SystemGeneratedIdentifier =
      useExport.NfGms.SystemGeneratedIdentifier;
    local.NfGcs.SystemGeneratedIdentifier =
      useExport.NfGcs.SystemGeneratedIdentifier;
    local.NfIcrch.SystemGeneratedIdentifier =
      useExport.NfIcrch.SystemGeneratedIdentifier;
    local.NfAcrch.SystemGeneratedIdentifier =
      useExport.NfAcrch.SystemGeneratedIdentifier;
    local.NfImj.SystemGeneratedIdentifier =
      useExport.NfImj.SystemGeneratedIdentifier;
    local.NfAmj.SystemGeneratedIdentifier =
      useExport.NfAmj.SystemGeneratedIdentifier;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public Collection Per
    {
      get => per ??= new();
      set => per = value;
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

    private Collection per;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType disbursementType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AfGmc.
    /// </summary>
    [JsonPropertyName("afGmc")]
    public DisbursementType AfGmc
    {
      get => afGmc ??= new();
      set => afGmc = value;
    }

    /// <summary>
    /// A value of AfGcs.
    /// </summary>
    [JsonPropertyName("afGcs")]
    public DisbursementType AfGcs
    {
      get => afGcs ??= new();
      set => afGcs = value;
    }

    /// <summary>
    /// A value of AfGms.
    /// </summary>
    [JsonPropertyName("afGms")]
    public DisbursementType AfGms
    {
      get => afGms ??= new();
      set => afGms = value;
    }

    /// <summary>
    /// A value of AfGsp.
    /// </summary>
    [JsonPropertyName("afGsp")]
    public DisbursementType AfGsp
    {
      get => afGsp ??= new();
      set => afGsp = value;
    }

    /// <summary>
    /// A value of AfiGmc.
    /// </summary>
    [JsonPropertyName("afiGmc")]
    public DisbursementType AfiGmc
    {
      get => afiGmc ??= new();
      set => afiGmc = value;
    }

    /// <summary>
    /// A value of AfiGcs.
    /// </summary>
    [JsonPropertyName("afiGcs")]
    public DisbursementType AfiGcs
    {
      get => afiGcs ??= new();
      set => afiGcs = value;
    }

    /// <summary>
    /// A value of AfiGms.
    /// </summary>
    [JsonPropertyName("afiGms")]
    public DisbursementType AfiGms
    {
      get => afiGms ??= new();
      set => afiGms = value;
    }

    /// <summary>
    /// A value of AfiGsp.
    /// </summary>
    [JsonPropertyName("afiGsp")]
    public DisbursementType AfiGsp
    {
      get => afiGsp ??= new();
      set => afiGsp = value;
    }

    /// <summary>
    /// A value of FcGmc.
    /// </summary>
    [JsonPropertyName("fcGmc")]
    public DisbursementType FcGmc
    {
      get => fcGmc ??= new();
      set => fcGmc = value;
    }

    /// <summary>
    /// A value of FcGms.
    /// </summary>
    [JsonPropertyName("fcGms")]
    public DisbursementType FcGms
    {
      get => fcGms ??= new();
      set => fcGms = value;
    }

    /// <summary>
    /// A value of FciGmc.
    /// </summary>
    [JsonPropertyName("fciGmc")]
    public DisbursementType FciGmc
    {
      get => fciGmc ??= new();
      set => fciGmc = value;
    }

    /// <summary>
    /// A value of FciGcs.
    /// </summary>
    [JsonPropertyName("fciGcs")]
    public DisbursementType FciGcs
    {
      get => fciGcs ??= new();
      set => fciGcs = value;
    }

    /// <summary>
    /// A value of FciGms.
    /// </summary>
    [JsonPropertyName("fciGms")]
    public DisbursementType FciGms
    {
      get => fciGms ??= new();
      set => fciGms = value;
    }

    /// <summary>
    /// A value of NaGmc.
    /// </summary>
    [JsonPropertyName("naGmc")]
    public DisbursementType NaGmc
    {
      get => naGmc ??= new();
      set => naGmc = value;
    }

    /// <summary>
    /// A value of NaGms.
    /// </summary>
    [JsonPropertyName("naGms")]
    public DisbursementType NaGms
    {
      get => naGms ??= new();
      set => naGms = value;
    }

    /// <summary>
    /// A value of NaGcs.
    /// </summary>
    [JsonPropertyName("naGcs")]
    public DisbursementType NaGcs
    {
      get => naGcs ??= new();
      set => naGcs = value;
    }

    /// <summary>
    /// A value of NaGsp.
    /// </summary>
    [JsonPropertyName("naGsp")]
    public DisbursementType NaGsp
    {
      get => naGsp ??= new();
      set => naGsp = value;
    }

    /// <summary>
    /// A value of NaiGmc.
    /// </summary>
    [JsonPropertyName("naiGmc")]
    public DisbursementType NaiGmc
    {
      get => naiGmc ??= new();
      set => naiGmc = value;
    }

    /// <summary>
    /// A value of NaiGms.
    /// </summary>
    [JsonPropertyName("naiGms")]
    public DisbursementType NaiGms
    {
      get => naiGms ??= new();
      set => naiGms = value;
    }

    /// <summary>
    /// A value of NaiGcs.
    /// </summary>
    [JsonPropertyName("naiGcs")]
    public DisbursementType NaiGcs
    {
      get => naiGcs ??= new();
      set => naiGcs = value;
    }

    /// <summary>
    /// A value of NaiGsp.
    /// </summary>
    [JsonPropertyName("naiGsp")]
    public DisbursementType NaiGsp
    {
      get => naiGsp ??= new();
      set => naiGsp = value;
    }

    /// <summary>
    /// A value of NcGmc.
    /// </summary>
    [JsonPropertyName("ncGmc")]
    public DisbursementType NcGmc
    {
      get => ncGmc ??= new();
      set => ncGmc = value;
    }

    /// <summary>
    /// A value of NcGcs.
    /// </summary>
    [JsonPropertyName("ncGcs")]
    public DisbursementType NcGcs
    {
      get => ncGcs ??= new();
      set => ncGcs = value;
    }

    /// <summary>
    /// A value of NcGms.
    /// </summary>
    [JsonPropertyName("ncGms")]
    public DisbursementType NcGms
    {
      get => ncGms ??= new();
      set => ncGms = value;
    }

    /// <summary>
    /// A value of NfGmc.
    /// </summary>
    [JsonPropertyName("nfGmc")]
    public DisbursementType NfGmc
    {
      get => nfGmc ??= new();
      set => nfGmc = value;
    }

    /// <summary>
    /// A value of NfGms.
    /// </summary>
    [JsonPropertyName("nfGms")]
    public DisbursementType NfGms
    {
      get => nfGms ??= new();
      set => nfGms = value;
    }

    /// <summary>
    /// A value of NfGcs.
    /// </summary>
    [JsonPropertyName("nfGcs")]
    public DisbursementType NfGcs
    {
      get => nfGcs ??= new();
      set => nfGcs = value;
    }

    /// <summary>
    /// A value of FcGcs.
    /// </summary>
    [JsonPropertyName("fcGcs")]
    public DisbursementType FcGcs
    {
      get => fcGcs ??= new();
      set => fcGcs = value;
    }

    /// <summary>
    /// A value of CrFeeObligationType.
    /// </summary>
    [JsonPropertyName("crFeeObligationType")]
    public ObligationType CrFeeObligationType
    {
      get => crFeeObligationType ??= new();
      set => crFeeObligationType = value;
    }

    /// <summary>
    /// A value of AfIms.
    /// </summary>
    [JsonPropertyName("afIms")]
    public DisbursementType AfIms
    {
      get => afIms ??= new();
      set => afIms = value;
    }

    /// <summary>
    /// A value of AfIsp.
    /// </summary>
    [JsonPropertyName("afIsp")]
    public DisbursementType AfIsp
    {
      get => afIsp ??= new();
      set => afIsp = value;
    }

    /// <summary>
    /// A value of AfAsp.
    /// </summary>
    [JsonPropertyName("afAsp")]
    public DisbursementType AfAsp
    {
      get => afAsp ??= new();
      set => afAsp = value;
    }

    /// <summary>
    /// A value of AfAms.
    /// </summary>
    [JsonPropertyName("afAms")]
    public DisbursementType AfAms
    {
      get => afAms ??= new();
      set => afAms = value;
    }

    /// <summary>
    /// A value of AfCcs.
    /// </summary>
    [JsonPropertyName("afCcs")]
    public DisbursementType AfCcs
    {
      get => afCcs ??= new();
      set => afCcs = value;
    }

    /// <summary>
    /// A value of AfAcs.
    /// </summary>
    [JsonPropertyName("afAcs")]
    public DisbursementType AfAcs
    {
      get => afAcs ??= new();
      set => afAcs = value;
    }

    /// <summary>
    /// A value of AfIcs.
    /// </summary>
    [JsonPropertyName("afIcs")]
    public DisbursementType AfIcs
    {
      get => afIcs ??= new();
      set => afIcs = value;
    }

    /// <summary>
    /// A value of AfCsp.
    /// </summary>
    [JsonPropertyName("afCsp")]
    public DisbursementType AfCsp
    {
      get => afCsp ??= new();
      set => afCsp = value;
    }

    /// <summary>
    /// A value of AfAsaj.
    /// </summary>
    [JsonPropertyName("afAsaj")]
    public DisbursementType AfAsaj
    {
      get => afAsaj ??= new();
      set => afAsaj = value;
    }

    /// <summary>
    /// A value of AfIsaj.
    /// </summary>
    [JsonPropertyName("afIsaj")]
    public DisbursementType AfIsaj
    {
      get => afIsaj ??= new();
      set => afIsaj = value;
    }

    /// <summary>
    /// A value of AfCms.
    /// </summary>
    [JsonPropertyName("afCms")]
    public DisbursementType AfCms
    {
      get => afCms ??= new();
      set => afCms = value;
    }

    /// <summary>
    /// A value of AfAmj.
    /// </summary>
    [JsonPropertyName("afAmj")]
    public DisbursementType AfAmj
    {
      get => afAmj ??= new();
      set => afAmj = value;
    }

    /// <summary>
    /// A value of AfImj.
    /// </summary>
    [JsonPropertyName("afImj")]
    public DisbursementType AfImj
    {
      get => afImj ??= new();
      set => afImj = value;
    }

    /// <summary>
    /// A value of AfiCms.
    /// </summary>
    [JsonPropertyName("afiCms")]
    public DisbursementType AfiCms
    {
      get => afiCms ??= new();
      set => afiCms = value;
    }

    /// <summary>
    /// A value of AfCmc.
    /// </summary>
    [JsonPropertyName("afCmc")]
    public DisbursementType AfCmc
    {
      get => afCmc ??= new();
      set => afCmc = value;
    }

    /// <summary>
    /// A value of AfAmc.
    /// </summary>
    [JsonPropertyName("afAmc")]
    public DisbursementType AfAmc
    {
      get => afAmc ??= new();
      set => afAmc = value;
    }

    /// <summary>
    /// A value of AfImc.
    /// </summary>
    [JsonPropertyName("afImc")]
    public DisbursementType AfImc
    {
      get => afImc ??= new();
      set => afImc = value;
    }

    /// <summary>
    /// A value of AfAaj.
    /// </summary>
    [JsonPropertyName("afAaj")]
    public DisbursementType AfAaj
    {
      get => afAaj ??= new();
      set => afAaj = value;
    }

    /// <summary>
    /// A value of AfIaj.
    /// </summary>
    [JsonPropertyName("afIaj")]
    public DisbursementType AfIaj
    {
      get => afIaj ??= new();
      set => afIaj = value;
    }

    /// <summary>
    /// A value of AfUme.
    /// </summary>
    [JsonPropertyName("afUme")]
    public DisbursementType AfUme
    {
      get => afUme ??= new();
      set => afUme = value;
    }

    /// <summary>
    /// A value of AfIume1.
    /// </summary>
    [JsonPropertyName("afIume1")]
    public DisbursementType AfIume1
    {
      get => afIume1 ??= new();
      set => afIume1 = value;
    }

    /// <summary>
    /// A value of AfIij.
    /// </summary>
    [JsonPropertyName("afIij")]
    public DisbursementType AfIij
    {
      get => afIij ??= new();
      set => afIij = value;
    }

    /// <summary>
    /// A value of AfAcrch.
    /// </summary>
    [JsonPropertyName("afAcrch")]
    public DisbursementType AfAcrch
    {
      get => afAcrch ??= new();
      set => afAcrch = value;
    }

    /// <summary>
    /// A value of AfIcrch.
    /// </summary>
    [JsonPropertyName("afIcrch")]
    public DisbursementType AfIcrch
    {
      get => afIcrch ??= new();
      set => afIcrch = value;
    }

    /// <summary>
    /// A value of AfVol.
    /// </summary>
    [JsonPropertyName("afVol")]
    public DisbursementType AfVol
    {
      get => afVol ??= new();
      set => afVol = value;
    }

    /// <summary>
    /// A value of AfVolR.
    /// </summary>
    [JsonPropertyName("afVolR")]
    public DisbursementType AfVolR
    {
      get => afVolR ??= new();
      set => afVolR = value;
    }

    /// <summary>
    /// A value of AfiCcs.
    /// </summary>
    [JsonPropertyName("afiCcs")]
    public DisbursementType AfiCcs
    {
      get => afiCcs ??= new();
      set => afiCcs = value;
    }

    /// <summary>
    /// A value of AfiAcs.
    /// </summary>
    [JsonPropertyName("afiAcs")]
    public DisbursementType AfiAcs
    {
      get => afiAcs ??= new();
      set => afiAcs = value;
    }

    /// <summary>
    /// A value of AfiIcs.
    /// </summary>
    [JsonPropertyName("afiIcs")]
    public DisbursementType AfiIcs
    {
      get => afiIcs ??= new();
      set => afiIcs = value;
    }

    /// <summary>
    /// A value of AfiCsp.
    /// </summary>
    [JsonPropertyName("afiCsp")]
    public DisbursementType AfiCsp
    {
      get => afiCsp ??= new();
      set => afiCsp = value;
    }

    /// <summary>
    /// A value of AfiIms.
    /// </summary>
    [JsonPropertyName("afiIms")]
    public DisbursementType AfiIms
    {
      get => afiIms ??= new();
      set => afiIms = value;
    }

    /// <summary>
    /// A value of AfiAms.
    /// </summary>
    [JsonPropertyName("afiAms")]
    public DisbursementType AfiAms
    {
      get => afiAms ??= new();
      set => afiAms = value;
    }

    /// <summary>
    /// A value of AfiIsp.
    /// </summary>
    [JsonPropertyName("afiIsp")]
    public DisbursementType AfiIsp
    {
      get => afiIsp ??= new();
      set => afiIsp = value;
    }

    /// <summary>
    /// A value of AfiAsp.
    /// </summary>
    [JsonPropertyName("afiAsp")]
    public DisbursementType AfiAsp
    {
      get => afiAsp ??= new();
      set => afiAsp = value;
    }

    /// <summary>
    /// A value of AfiAsaj.
    /// </summary>
    [JsonPropertyName("afiAsaj")]
    public DisbursementType AfiAsaj
    {
      get => afiAsaj ??= new();
      set => afiAsaj = value;
    }

    /// <summary>
    /// A value of AfiIsaj.
    /// </summary>
    [JsonPropertyName("afiIsaj")]
    public DisbursementType AfiIsaj
    {
      get => afiIsaj ??= new();
      set => afiIsaj = value;
    }

    /// <summary>
    /// A value of AfiAmj.
    /// </summary>
    [JsonPropertyName("afiAmj")]
    public DisbursementType AfiAmj
    {
      get => afiAmj ??= new();
      set => afiAmj = value;
    }

    /// <summary>
    /// A value of AfiImj.
    /// </summary>
    [JsonPropertyName("afiImj")]
    public DisbursementType AfiImj
    {
      get => afiImj ??= new();
      set => afiImj = value;
    }

    /// <summary>
    /// A value of AfiCmc.
    /// </summary>
    [JsonPropertyName("afiCmc")]
    public DisbursementType AfiCmc
    {
      get => afiCmc ??= new();
      set => afiCmc = value;
    }

    /// <summary>
    /// A value of AfiAmc.
    /// </summary>
    [JsonPropertyName("afiAmc")]
    public DisbursementType AfiAmc
    {
      get => afiAmc ??= new();
      set => afiAmc = value;
    }

    /// <summary>
    /// A value of AfiImc.
    /// </summary>
    [JsonPropertyName("afiImc")]
    public DisbursementType AfiImc
    {
      get => afiImc ??= new();
      set => afiImc = value;
    }

    /// <summary>
    /// A value of AfiAaj.
    /// </summary>
    [JsonPropertyName("afiAaj")]
    public DisbursementType AfiAaj
    {
      get => afiAaj ??= new();
      set => afiAaj = value;
    }

    /// <summary>
    /// A value of AfiIaj.
    /// </summary>
    [JsonPropertyName("afiIaj")]
    public DisbursementType AfiIaj
    {
      get => afiIaj ??= new();
      set => afiIaj = value;
    }

    /// <summary>
    /// A value of AfiUme2.
    /// </summary>
    [JsonPropertyName("afiUme2")]
    public DisbursementType AfiUme2
    {
      get => afiUme2 ??= new();
      set => afiUme2 = value;
    }

    /// <summary>
    /// A value of AfiIume.
    /// </summary>
    [JsonPropertyName("afiIume")]
    public DisbursementType AfiIume
    {
      get => afiIume ??= new();
      set => afiIume = value;
    }

    /// <summary>
    /// A value of AfiIij.
    /// </summary>
    [JsonPropertyName("afiIij")]
    public DisbursementType AfiIij
    {
      get => afiIij ??= new();
      set => afiIij = value;
    }

    /// <summary>
    /// A value of AfiAcrch.
    /// </summary>
    [JsonPropertyName("afiAcrch")]
    public DisbursementType AfiAcrch
    {
      get => afiAcrch ??= new();
      set => afiAcrch = value;
    }

    /// <summary>
    /// A value of AfiIcrch.
    /// </summary>
    [JsonPropertyName("afiIcrch")]
    public DisbursementType AfiIcrch
    {
      get => afiIcrch ??= new();
      set => afiIcrch = value;
    }

    /// <summary>
    /// A value of AfiVol.
    /// </summary>
    [JsonPropertyName("afiVol")]
    public DisbursementType AfiVol
    {
      get => afiVol ??= new();
      set => afiVol = value;
    }

    /// <summary>
    /// A value of AfiVolR.
    /// </summary>
    [JsonPropertyName("afiVolR")]
    public DisbursementType AfiVolR
    {
      get => afiVolR ??= new();
      set => afiVolR = value;
    }

    /// <summary>
    /// A value of Fc.
    /// </summary>
    [JsonPropertyName("fc")]
    public Collection Fc
    {
      get => fc ??= new();
      set => fc = value;
    }

    /// <summary>
    /// A value of FcCcs.
    /// </summary>
    [JsonPropertyName("fcCcs")]
    public DisbursementType FcCcs
    {
      get => fcCcs ??= new();
      set => fcCcs = value;
    }

    /// <summary>
    /// A value of FcAcs.
    /// </summary>
    [JsonPropertyName("fcAcs")]
    public DisbursementType FcAcs
    {
      get => fcAcs ??= new();
      set => fcAcs = value;
    }

    /// <summary>
    /// A value of FcIcs.
    /// </summary>
    [JsonPropertyName("fcIcs")]
    public DisbursementType FcIcs
    {
      get => fcIcs ??= new();
      set => fcIcs = value;
    }

    /// <summary>
    /// A value of FcIms.
    /// </summary>
    [JsonPropertyName("fcIms")]
    public DisbursementType FcIms
    {
      get => fcIms ??= new();
      set => fcIms = value;
    }

    /// <summary>
    /// A value of FcAms.
    /// </summary>
    [JsonPropertyName("fcAms")]
    public DisbursementType FcAms
    {
      get => fcAms ??= new();
      set => fcAms = value;
    }

    /// <summary>
    /// A value of FcCms.
    /// </summary>
    [JsonPropertyName("fcCms")]
    public DisbursementType FcCms
    {
      get => fcCms ??= new();
      set => fcCms = value;
    }

    /// <summary>
    /// A value of FcAmj.
    /// </summary>
    [JsonPropertyName("fcAmj")]
    public DisbursementType FcAmj
    {
      get => fcAmj ??= new();
      set => fcAmj = value;
    }

    /// <summary>
    /// A value of FcImj.
    /// </summary>
    [JsonPropertyName("fcImj")]
    public DisbursementType FcImj
    {
      get => fcImj ??= new();
      set => fcImj = value;
    }

    /// <summary>
    /// A value of FcCmc.
    /// </summary>
    [JsonPropertyName("fcCmc")]
    public DisbursementType FcCmc
    {
      get => fcCmc ??= new();
      set => fcCmc = value;
    }

    /// <summary>
    /// A value of FcAmc.
    /// </summary>
    [JsonPropertyName("fcAmc")]
    public DisbursementType FcAmc
    {
      get => fcAmc ??= new();
      set => fcAmc = value;
    }

    /// <summary>
    /// A value of FcImc.
    /// </summary>
    [JsonPropertyName("fcImc")]
    public DisbursementType FcImc
    {
      get => fcImc ??= new();
      set => fcImc = value;
    }

    /// <summary>
    /// A value of FcAaj.
    /// </summary>
    [JsonPropertyName("fcAaj")]
    public DisbursementType FcAaj
    {
      get => fcAaj ??= new();
      set => fcAaj = value;
    }

    /// <summary>
    /// A value of FcIaj.
    /// </summary>
    [JsonPropertyName("fcIaj")]
    public DisbursementType FcIaj
    {
      get => fcIaj ??= new();
      set => fcIaj = value;
    }

    /// <summary>
    /// A value of FcUme.
    /// </summary>
    [JsonPropertyName("fcUme")]
    public DisbursementType FcUme
    {
      get => fcUme ??= new();
      set => fcUme = value;
    }

    /// <summary>
    /// A value of FcIume1.
    /// </summary>
    [JsonPropertyName("fcIume1")]
    public DisbursementType FcIume1
    {
      get => fcIume1 ??= new();
      set => fcIume1 = value;
    }

    /// <summary>
    /// A value of FcIij.
    /// </summary>
    [JsonPropertyName("fcIij")]
    public DisbursementType FcIij
    {
      get => fcIij ??= new();
      set => fcIij = value;
    }

    /// <summary>
    /// A value of FcAcrch.
    /// </summary>
    [JsonPropertyName("fcAcrch")]
    public DisbursementType FcAcrch
    {
      get => fcAcrch ??= new();
      set => fcAcrch = value;
    }

    /// <summary>
    /// A value of FcIcrch.
    /// </summary>
    [JsonPropertyName("fcIcrch")]
    public DisbursementType FcIcrch
    {
      get => fcIcrch ??= new();
      set => fcIcrch = value;
    }

    /// <summary>
    /// A value of Fci.
    /// </summary>
    [JsonPropertyName("fci")]
    public Collection Fci
    {
      get => fci ??= new();
      set => fci = value;
    }

    /// <summary>
    /// A value of FciCcs.
    /// </summary>
    [JsonPropertyName("fciCcs")]
    public DisbursementType FciCcs
    {
      get => fciCcs ??= new();
      set => fciCcs = value;
    }

    /// <summary>
    /// A value of FciAcs.
    /// </summary>
    [JsonPropertyName("fciAcs")]
    public DisbursementType FciAcs
    {
      get => fciAcs ??= new();
      set => fciAcs = value;
    }

    /// <summary>
    /// A value of FciIcs.
    /// </summary>
    [JsonPropertyName("fciIcs")]
    public DisbursementType FciIcs
    {
      get => fciIcs ??= new();
      set => fciIcs = value;
    }

    /// <summary>
    /// A value of FciCms.
    /// </summary>
    [JsonPropertyName("fciCms")]
    public DisbursementType FciCms
    {
      get => fciCms ??= new();
      set => fciCms = value;
    }

    /// <summary>
    /// A value of FciAmj.
    /// </summary>
    [JsonPropertyName("fciAmj")]
    public DisbursementType FciAmj
    {
      get => fciAmj ??= new();
      set => fciAmj = value;
    }

    /// <summary>
    /// A value of FciImj.
    /// </summary>
    [JsonPropertyName("fciImj")]
    public DisbursementType FciImj
    {
      get => fciImj ??= new();
      set => fciImj = value;
    }

    /// <summary>
    /// A value of FciCmc.
    /// </summary>
    [JsonPropertyName("fciCmc")]
    public DisbursementType FciCmc
    {
      get => fciCmc ??= new();
      set => fciCmc = value;
    }

    /// <summary>
    /// A value of FciIms.
    /// </summary>
    [JsonPropertyName("fciIms")]
    public DisbursementType FciIms
    {
      get => fciIms ??= new();
      set => fciIms = value;
    }

    /// <summary>
    /// A value of FciAms.
    /// </summary>
    [JsonPropertyName("fciAms")]
    public DisbursementType FciAms
    {
      get => fciAms ??= new();
      set => fciAms = value;
    }

    /// <summary>
    /// A value of FciAmc.
    /// </summary>
    [JsonPropertyName("fciAmc")]
    public DisbursementType FciAmc
    {
      get => fciAmc ??= new();
      set => fciAmc = value;
    }

    /// <summary>
    /// A value of FciImc.
    /// </summary>
    [JsonPropertyName("fciImc")]
    public DisbursementType FciImc
    {
      get => fciImc ??= new();
      set => fciImc = value;
    }

    /// <summary>
    /// A value of FciAaj.
    /// </summary>
    [JsonPropertyName("fciAaj")]
    public DisbursementType FciAaj
    {
      get => fciAaj ??= new();
      set => fciAaj = value;
    }

    /// <summary>
    /// A value of FciIaj.
    /// </summary>
    [JsonPropertyName("fciIaj")]
    public DisbursementType FciIaj
    {
      get => fciIaj ??= new();
      set => fciIaj = value;
    }

    /// <summary>
    /// A value of FciUme2.
    /// </summary>
    [JsonPropertyName("fciUme2")]
    public DisbursementType FciUme2
    {
      get => fciUme2 ??= new();
      set => fciUme2 = value;
    }

    /// <summary>
    /// A value of FciIume.
    /// </summary>
    [JsonPropertyName("fciIume")]
    public DisbursementType FciIume
    {
      get => fciIume ??= new();
      set => fciIume = value;
    }

    /// <summary>
    /// A value of FciIij.
    /// </summary>
    [JsonPropertyName("fciIij")]
    public DisbursementType FciIij
    {
      get => fciIij ??= new();
      set => fciIij = value;
    }

    /// <summary>
    /// A value of FciAcrch.
    /// </summary>
    [JsonPropertyName("fciAcrch")]
    public DisbursementType FciAcrch
    {
      get => fciAcrch ??= new();
      set => fciAcrch = value;
    }

    /// <summary>
    /// A value of FciIcrch.
    /// </summary>
    [JsonPropertyName("fciIcrch")]
    public DisbursementType FciIcrch
    {
      get => fciIcrch ??= new();
      set => fciIcrch = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Collection Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of NaCcs.
    /// </summary>
    [JsonPropertyName("naCcs")]
    public DisbursementType NaCcs
    {
      get => naCcs ??= new();
      set => naCcs = value;
    }

    /// <summary>
    /// A value of NaAcs.
    /// </summary>
    [JsonPropertyName("naAcs")]
    public DisbursementType NaAcs
    {
      get => naAcs ??= new();
      set => naAcs = value;
    }

    /// <summary>
    /// A value of NaIcs.
    /// </summary>
    [JsonPropertyName("naIcs")]
    public DisbursementType NaIcs
    {
      get => naIcs ??= new();
      set => naIcs = value;
    }

    /// <summary>
    /// A value of NaCcsr.
    /// </summary>
    [JsonPropertyName("naCcsr")]
    public DisbursementType NaCcsr
    {
      get => naCcsr ??= new();
      set => naCcsr = value;
    }

    /// <summary>
    /// A value of NaAcsr.
    /// </summary>
    [JsonPropertyName("naAcsr")]
    public DisbursementType NaAcsr
    {
      get => naAcsr ??= new();
      set => naAcsr = value;
    }

    /// <summary>
    /// A value of NaIcsr.
    /// </summary>
    [JsonPropertyName("naIcsr")]
    public DisbursementType NaIcsr
    {
      get => naIcsr ??= new();
      set => naIcsr = value;
    }

    /// <summary>
    /// A value of NaCsp.
    /// </summary>
    [JsonPropertyName("naCsp")]
    public DisbursementType NaCsp
    {
      get => naCsp ??= new();
      set => naCsp = value;
    }

    /// <summary>
    /// A value of NaAsaj.
    /// </summary>
    [JsonPropertyName("naAsaj")]
    public DisbursementType NaAsaj
    {
      get => naAsaj ??= new();
      set => naAsaj = value;
    }

    /// <summary>
    /// A value of NaIsaj.
    /// </summary>
    [JsonPropertyName("naIsaj")]
    public DisbursementType NaIsaj
    {
      get => naIsaj ??= new();
      set => naIsaj = value;
    }

    /// <summary>
    /// A value of NaCspr.
    /// </summary>
    [JsonPropertyName("naCspr")]
    public DisbursementType NaCspr
    {
      get => naCspr ??= new();
      set => naCspr = value;
    }

    /// <summary>
    /// A value of NaAspr.
    /// </summary>
    [JsonPropertyName("naAspr")]
    public DisbursementType NaAspr
    {
      get => naAspr ??= new();
      set => naAspr = value;
    }

    /// <summary>
    /// A value of NaIspr.
    /// </summary>
    [JsonPropertyName("naIspr")]
    public DisbursementType NaIspr
    {
      get => naIspr ??= new();
      set => naIspr = value;
    }

    /// <summary>
    /// A value of NaCms.
    /// </summary>
    [JsonPropertyName("naCms")]
    public DisbursementType NaCms
    {
      get => naCms ??= new();
      set => naCms = value;
    }

    /// <summary>
    /// A value of NaAmj.
    /// </summary>
    [JsonPropertyName("naAmj")]
    public DisbursementType NaAmj
    {
      get => naAmj ??= new();
      set => naAmj = value;
    }

    /// <summary>
    /// A value of NaImj.
    /// </summary>
    [JsonPropertyName("naImj")]
    public DisbursementType NaImj
    {
      get => naImj ??= new();
      set => naImj = value;
    }

    /// <summary>
    /// A value of NaCmsr.
    /// </summary>
    [JsonPropertyName("naCmsr")]
    public DisbursementType NaCmsr
    {
      get => naCmsr ??= new();
      set => naCmsr = value;
    }

    /// <summary>
    /// A value of NaAmsr.
    /// </summary>
    [JsonPropertyName("naAmsr")]
    public DisbursementType NaAmsr
    {
      get => naAmsr ??= new();
      set => naAmsr = value;
    }

    /// <summary>
    /// A value of NaImsr.
    /// </summary>
    [JsonPropertyName("naImsr")]
    public DisbursementType NaImsr
    {
      get => naImsr ??= new();
      set => naImsr = value;
    }

    /// <summary>
    /// A value of NaCmc.
    /// </summary>
    [JsonPropertyName("naCmc")]
    public DisbursementType NaCmc
    {
      get => naCmc ??= new();
      set => naCmc = value;
    }

    /// <summary>
    /// A value of NaAmc.
    /// </summary>
    [JsonPropertyName("naAmc")]
    public DisbursementType NaAmc
    {
      get => naAmc ??= new();
      set => naAmc = value;
    }

    /// <summary>
    /// A value of NaImc.
    /// </summary>
    [JsonPropertyName("naImc")]
    public DisbursementType NaImc
    {
      get => naImc ??= new();
      set => naImc = value;
    }

    /// <summary>
    /// A value of NaCmcr.
    /// </summary>
    [JsonPropertyName("naCmcr")]
    public DisbursementType NaCmcr
    {
      get => naCmcr ??= new();
      set => naCmcr = value;
    }

    /// <summary>
    /// A value of NaAmcr.
    /// </summary>
    [JsonPropertyName("naAmcr")]
    public DisbursementType NaAmcr
    {
      get => naAmcr ??= new();
      set => naAmcr = value;
    }

    /// <summary>
    /// A value of NaImcr.
    /// </summary>
    [JsonPropertyName("naImcr")]
    public DisbursementType NaImcr
    {
      get => naImcr ??= new();
      set => naImcr = value;
    }

    /// <summary>
    /// A value of NaIsp.
    /// </summary>
    [JsonPropertyName("naIsp")]
    public DisbursementType NaIsp
    {
      get => naIsp ??= new();
      set => naIsp = value;
    }

    /// <summary>
    /// A value of NaAsp.
    /// </summary>
    [JsonPropertyName("naAsp")]
    public DisbursementType NaAsp
    {
      get => naAsp ??= new();
      set => naAsp = value;
    }

    /// <summary>
    /// A value of NaAaj.
    /// </summary>
    [JsonPropertyName("naAaj")]
    public DisbursementType NaAaj
    {
      get => naAaj ??= new();
      set => naAaj = value;
    }

    /// <summary>
    /// A value of NaIarj.
    /// </summary>
    [JsonPropertyName("naIarj")]
    public DisbursementType NaIarj
    {
      get => naIarj ??= new();
      set => naIarj = value;
    }

    /// <summary>
    /// A value of NaArrjr.
    /// </summary>
    [JsonPropertyName("naArrjr")]
    public DisbursementType NaArrjr
    {
      get => naArrjr ??= new();
      set => naArrjr = value;
    }

    /// <summary>
    /// A value of NaIaj.
    /// </summary>
    [JsonPropertyName("naIaj")]
    public DisbursementType NaIaj
    {
      get => naIaj ??= new();
      set => naIaj = value;
    }

    /// <summary>
    /// A value of NaUme.
    /// </summary>
    [JsonPropertyName("naUme")]
    public DisbursementType NaUme
    {
      get => naUme ??= new();
      set => naUme = value;
    }

    /// <summary>
    /// A value of NaIume1.
    /// </summary>
    [JsonPropertyName("naIume1")]
    public DisbursementType NaIume1
    {
      get => naIume1 ??= new();
      set => naIume1 = value;
    }

    /// <summary>
    /// A value of NaUmer.
    /// </summary>
    [JsonPropertyName("naUmer")]
    public DisbursementType NaUmer
    {
      get => naUmer ??= new();
      set => naUmer = value;
    }

    /// <summary>
    /// A value of NaIms.
    /// </summary>
    [JsonPropertyName("naIms")]
    public DisbursementType NaIms
    {
      get => naIms ??= new();
      set => naIms = value;
    }

    /// <summary>
    /// A value of NaAms.
    /// </summary>
    [JsonPropertyName("naAms")]
    public DisbursementType NaAms
    {
      get => naAms ??= new();
      set => naAms = value;
    }

    /// <summary>
    /// A value of NaIumer1.
    /// </summary>
    [JsonPropertyName("naIumer1")]
    public DisbursementType NaIumer1
    {
      get => naIumer1 ??= new();
      set => naIumer1 = value;
    }

    /// <summary>
    /// A value of NaIij.
    /// </summary>
    [JsonPropertyName("naIij")]
    public DisbursementType NaIij
    {
      get => naIij ??= new();
      set => naIij = value;
    }

    /// <summary>
    /// A value of NaIntjr.
    /// </summary>
    [JsonPropertyName("naIntjr")]
    public DisbursementType NaIntjr
    {
      get => naIntjr ??= new();
      set => naIntjr = value;
    }

    /// <summary>
    /// A value of NaAcrch.
    /// </summary>
    [JsonPropertyName("naAcrch")]
    public DisbursementType NaAcrch
    {
      get => naAcrch ??= new();
      set => naAcrch = value;
    }

    /// <summary>
    /// A value of NaIcrch.
    /// </summary>
    [JsonPropertyName("naIcrch")]
    public DisbursementType NaIcrch
    {
      get => naIcrch ??= new();
      set => naIcrch = value;
    }

    /// <summary>
    /// A value of NaCrcr.
    /// </summary>
    [JsonPropertyName("naCrcr")]
    public DisbursementType NaCrcr
    {
      get => naCrcr ??= new();
      set => naCrcr = value;
    }

    /// <summary>
    /// A value of NaIcrcr1.
    /// </summary>
    [JsonPropertyName("naIcrcr1")]
    public DisbursementType NaIcrcr1
    {
      get => naIcrcr1 ??= new();
      set => naIcrcr1 = value;
    }

    /// <summary>
    /// A value of NaVol.
    /// </summary>
    [JsonPropertyName("naVol")]
    public DisbursementType NaVol
    {
      get => naVol ??= new();
      set => naVol = value;
    }

    /// <summary>
    /// A value of NaVolR.
    /// </summary>
    [JsonPropertyName("naVolR")]
    public DisbursementType NaVolR
    {
      get => naVolR ??= new();
      set => naVolR = value;
    }

    /// <summary>
    /// A value of NaiCcs.
    /// </summary>
    [JsonPropertyName("naiCcs")]
    public DisbursementType NaiCcs
    {
      get => naiCcs ??= new();
      set => naiCcs = value;
    }

    /// <summary>
    /// A value of NaiAcs.
    /// </summary>
    [JsonPropertyName("naiAcs")]
    public DisbursementType NaiAcs
    {
      get => naiAcs ??= new();
      set => naiAcs = value;
    }

    /// <summary>
    /// A value of NaiIms.
    /// </summary>
    [JsonPropertyName("naiIms")]
    public DisbursementType NaiIms
    {
      get => naiIms ??= new();
      set => naiIms = value;
    }

    /// <summary>
    /// A value of NaiAms.
    /// </summary>
    [JsonPropertyName("naiAms")]
    public DisbursementType NaiAms
    {
      get => naiAms ??= new();
      set => naiAms = value;
    }

    /// <summary>
    /// A value of NaiIsp.
    /// </summary>
    [JsonPropertyName("naiIsp")]
    public DisbursementType NaiIsp
    {
      get => naiIsp ??= new();
      set => naiIsp = value;
    }

    /// <summary>
    /// A value of NaiAsp.
    /// </summary>
    [JsonPropertyName("naiAsp")]
    public DisbursementType NaiAsp
    {
      get => naiAsp ??= new();
      set => naiAsp = value;
    }

    /// <summary>
    /// A value of NaiIcs.
    /// </summary>
    [JsonPropertyName("naiIcs")]
    public DisbursementType NaiIcs
    {
      get => naiIcs ??= new();
      set => naiIcs = value;
    }

    /// <summary>
    /// A value of NaiCcsr.
    /// </summary>
    [JsonPropertyName("naiCcsr")]
    public DisbursementType NaiCcsr
    {
      get => naiCcsr ??= new();
      set => naiCcsr = value;
    }

    /// <summary>
    /// A value of NaiAcsr.
    /// </summary>
    [JsonPropertyName("naiAcsr")]
    public DisbursementType NaiAcsr
    {
      get => naiAcsr ??= new();
      set => naiAcsr = value;
    }

    /// <summary>
    /// A value of NaiIcsr.
    /// </summary>
    [JsonPropertyName("naiIcsr")]
    public DisbursementType NaiIcsr
    {
      get => naiIcsr ??= new();
      set => naiIcsr = value;
    }

    /// <summary>
    /// A value of NaiCsp.
    /// </summary>
    [JsonPropertyName("naiCsp")]
    public DisbursementType NaiCsp
    {
      get => naiCsp ??= new();
      set => naiCsp = value;
    }

    /// <summary>
    /// A value of NaiAsaj.
    /// </summary>
    [JsonPropertyName("naiAsaj")]
    public DisbursementType NaiAsaj
    {
      get => naiAsaj ??= new();
      set => naiAsaj = value;
    }

    /// <summary>
    /// A value of NaiIsaj.
    /// </summary>
    [JsonPropertyName("naiIsaj")]
    public DisbursementType NaiIsaj
    {
      get => naiIsaj ??= new();
      set => naiIsaj = value;
    }

    /// <summary>
    /// A value of NaiCspr.
    /// </summary>
    [JsonPropertyName("naiCspr")]
    public DisbursementType NaiCspr
    {
      get => naiCspr ??= new();
      set => naiCspr = value;
    }

    /// <summary>
    /// A value of NaiAspr.
    /// </summary>
    [JsonPropertyName("naiAspr")]
    public DisbursementType NaiAspr
    {
      get => naiAspr ??= new();
      set => naiAspr = value;
    }

    /// <summary>
    /// A value of NaiIspr.
    /// </summary>
    [JsonPropertyName("naiIspr")]
    public DisbursementType NaiIspr
    {
      get => naiIspr ??= new();
      set => naiIspr = value;
    }

    /// <summary>
    /// A value of NaiCms.
    /// </summary>
    [JsonPropertyName("naiCms")]
    public DisbursementType NaiCms
    {
      get => naiCms ??= new();
      set => naiCms = value;
    }

    /// <summary>
    /// A value of NaiAmj.
    /// </summary>
    [JsonPropertyName("naiAmj")]
    public DisbursementType NaiAmj
    {
      get => naiAmj ??= new();
      set => naiAmj = value;
    }

    /// <summary>
    /// A value of NaiImj.
    /// </summary>
    [JsonPropertyName("naiImj")]
    public DisbursementType NaiImj
    {
      get => naiImj ??= new();
      set => naiImj = value;
    }

    /// <summary>
    /// A value of NaiCmsr.
    /// </summary>
    [JsonPropertyName("naiCmsr")]
    public DisbursementType NaiCmsr
    {
      get => naiCmsr ??= new();
      set => naiCmsr = value;
    }

    /// <summary>
    /// A value of NaiAmsr.
    /// </summary>
    [JsonPropertyName("naiAmsr")]
    public DisbursementType NaiAmsr
    {
      get => naiAmsr ??= new();
      set => naiAmsr = value;
    }

    /// <summary>
    /// A value of NaiImsr.
    /// </summary>
    [JsonPropertyName("naiImsr")]
    public DisbursementType NaiImsr
    {
      get => naiImsr ??= new();
      set => naiImsr = value;
    }

    /// <summary>
    /// A value of NaiCmc.
    /// </summary>
    [JsonPropertyName("naiCmc")]
    public DisbursementType NaiCmc
    {
      get => naiCmc ??= new();
      set => naiCmc = value;
    }

    /// <summary>
    /// A value of NaiAmc.
    /// </summary>
    [JsonPropertyName("naiAmc")]
    public DisbursementType NaiAmc
    {
      get => naiAmc ??= new();
      set => naiAmc = value;
    }

    /// <summary>
    /// A value of NaiImc.
    /// </summary>
    [JsonPropertyName("naiImc")]
    public DisbursementType NaiImc
    {
      get => naiImc ??= new();
      set => naiImc = value;
    }

    /// <summary>
    /// A value of NaiCmcr.
    /// </summary>
    [JsonPropertyName("naiCmcr")]
    public DisbursementType NaiCmcr
    {
      get => naiCmcr ??= new();
      set => naiCmcr = value;
    }

    /// <summary>
    /// A value of NaiAmcr.
    /// </summary>
    [JsonPropertyName("naiAmcr")]
    public DisbursementType NaiAmcr
    {
      get => naiAmcr ??= new();
      set => naiAmcr = value;
    }

    /// <summary>
    /// A value of NaiImcr.
    /// </summary>
    [JsonPropertyName("naiImcr")]
    public DisbursementType NaiImcr
    {
      get => naiImcr ??= new();
      set => naiImcr = value;
    }

    /// <summary>
    /// A value of NaiAaj.
    /// </summary>
    [JsonPropertyName("naiAaj")]
    public DisbursementType NaiAaj
    {
      get => naiAaj ??= new();
      set => naiAaj = value;
    }

    /// <summary>
    /// A value of NaiIarj.
    /// </summary>
    [JsonPropertyName("naiIarj")]
    public DisbursementType NaiIarj
    {
      get => naiIarj ??= new();
      set => naiIarj = value;
    }

    /// <summary>
    /// A value of NaiArrjr.
    /// </summary>
    [JsonPropertyName("naiArrjr")]
    public DisbursementType NaiArrjr
    {
      get => naiArrjr ??= new();
      set => naiArrjr = value;
    }

    /// <summary>
    /// A value of NaiIaj.
    /// </summary>
    [JsonPropertyName("naiIaj")]
    public DisbursementType NaiIaj
    {
      get => naiIaj ??= new();
      set => naiIaj = value;
    }

    /// <summary>
    /// A value of Nai.
    /// </summary>
    [JsonPropertyName("nai")]
    public Collection Nai
    {
      get => nai ??= new();
      set => nai = value;
    }

    /// <summary>
    /// A value of NaiUme2.
    /// </summary>
    [JsonPropertyName("naiUme2")]
    public DisbursementType NaiUme2
    {
      get => naiUme2 ??= new();
      set => naiUme2 = value;
    }

    /// <summary>
    /// A value of NaiIume.
    /// </summary>
    [JsonPropertyName("naiIume")]
    public DisbursementType NaiIume
    {
      get => naiIume ??= new();
      set => naiIume = value;
    }

    /// <summary>
    /// A value of NaiUmer2.
    /// </summary>
    [JsonPropertyName("naiUmer2")]
    public DisbursementType NaiUmer2
    {
      get => naiUmer2 ??= new();
      set => naiUmer2 = value;
    }

    /// <summary>
    /// A value of NaiIumer.
    /// </summary>
    [JsonPropertyName("naiIumer")]
    public DisbursementType NaiIumer
    {
      get => naiIumer ??= new();
      set => naiIumer = value;
    }

    /// <summary>
    /// A value of NaiIij.
    /// </summary>
    [JsonPropertyName("naiIij")]
    public DisbursementType NaiIij
    {
      get => naiIij ??= new();
      set => naiIij = value;
    }

    /// <summary>
    /// A value of NaiIntjr.
    /// </summary>
    [JsonPropertyName("naiIntjr")]
    public DisbursementType NaiIntjr
    {
      get => naiIntjr ??= new();
      set => naiIntjr = value;
    }

    /// <summary>
    /// A value of NaiAcrch.
    /// </summary>
    [JsonPropertyName("naiAcrch")]
    public DisbursementType NaiAcrch
    {
      get => naiAcrch ??= new();
      set => naiAcrch = value;
    }

    /// <summary>
    /// A value of NaiIcrci.
    /// </summary>
    [JsonPropertyName("naiIcrci")]
    public DisbursementType NaiIcrci
    {
      get => naiIcrci ??= new();
      set => naiIcrci = value;
    }

    /// <summary>
    /// A value of NaiCrcr2.
    /// </summary>
    [JsonPropertyName("naiCrcr2")]
    public DisbursementType NaiCrcr2
    {
      get => naiCrcr2 ??= new();
      set => naiCrcr2 = value;
    }

    /// <summary>
    /// A value of NaiIcrcr.
    /// </summary>
    [JsonPropertyName("naiIcrcr")]
    public DisbursementType NaiIcrcr
    {
      get => naiIcrcr ??= new();
      set => naiIcrcr = value;
    }

    /// <summary>
    /// A value of NaiVol.
    /// </summary>
    [JsonPropertyName("naiVol")]
    public DisbursementType NaiVol
    {
      get => naiVol ??= new();
      set => naiVol = value;
    }

    /// <summary>
    /// A value of NaiVolR.
    /// </summary>
    [JsonPropertyName("naiVolR")]
    public DisbursementType NaiVolR
    {
      get => naiVolR ??= new();
      set => naiVolR = value;
    }

    /// <summary>
    /// A value of NaiIcrch.
    /// </summary>
    [JsonPropertyName("naiIcrch")]
    public DisbursementType NaiIcrch
    {
      get => naiIcrch ??= new();
      set => naiIcrch = value;
    }

    /// <summary>
    /// A value of NcIij.
    /// </summary>
    [JsonPropertyName("ncIij")]
    public DisbursementType NcIij
    {
      get => ncIij ??= new();
      set => ncIij = value;
    }

    /// <summary>
    /// A value of NcAcrch.
    /// </summary>
    [JsonPropertyName("ncAcrch")]
    public DisbursementType NcAcrch
    {
      get => ncAcrch ??= new();
      set => ncAcrch = value;
    }

    /// <summary>
    /// A value of NcIcrch.
    /// </summary>
    [JsonPropertyName("ncIcrch")]
    public DisbursementType NcIcrch
    {
      get => ncIcrch ??= new();
      set => ncIcrch = value;
    }

    /// <summary>
    /// A value of NcIume.
    /// </summary>
    [JsonPropertyName("ncIume")]
    public DisbursementType NcIume
    {
      get => ncIume ??= new();
      set => ncIume = value;
    }

    /// <summary>
    /// A value of NcUme.
    /// </summary>
    [JsonPropertyName("ncUme")]
    public DisbursementType NcUme
    {
      get => ncUme ??= new();
      set => ncUme = value;
    }

    /// <summary>
    /// A value of NcIsp.
    /// </summary>
    [JsonPropertyName("ncIsp")]
    public DisbursementType NcIsp
    {
      get => ncIsp ??= new();
      set => ncIsp = value;
    }

    /// <summary>
    /// A value of NcAsp.
    /// </summary>
    [JsonPropertyName("ncAsp")]
    public DisbursementType NcAsp
    {
      get => ncAsp ??= new();
      set => ncAsp = value;
    }

    /// <summary>
    /// A value of NcIarj.
    /// </summary>
    [JsonPropertyName("ncIarj")]
    public DisbursementType NcIarj
    {
      get => ncIarj ??= new();
      set => ncIarj = value;
    }

    /// <summary>
    /// A value of NcAaj.
    /// </summary>
    [JsonPropertyName("ncAaj")]
    public DisbursementType NcAaj
    {
      get => ncAaj ??= new();
      set => ncAaj = value;
    }

    /// <summary>
    /// A value of NcImc.
    /// </summary>
    [JsonPropertyName("ncImc")]
    public DisbursementType NcImc
    {
      get => ncImc ??= new();
      set => ncImc = value;
    }

    /// <summary>
    /// A value of NcAmc.
    /// </summary>
    [JsonPropertyName("ncAmc")]
    public DisbursementType NcAmc
    {
      get => ncAmc ??= new();
      set => ncAmc = value;
    }

    /// <summary>
    /// A value of NcCmc.
    /// </summary>
    [JsonPropertyName("ncCmc")]
    public DisbursementType NcCmc
    {
      get => ncCmc ??= new();
      set => ncCmc = value;
    }

    /// <summary>
    /// A value of NcImj.
    /// </summary>
    [JsonPropertyName("ncImj")]
    public DisbursementType NcImj
    {
      get => ncImj ??= new();
      set => ncImj = value;
    }

    /// <summary>
    /// A value of NcAmj.
    /// </summary>
    [JsonPropertyName("ncAmj")]
    public DisbursementType NcAmj
    {
      get => ncAmj ??= new();
      set => ncAmj = value;
    }

    /// <summary>
    /// A value of NcCms.
    /// </summary>
    [JsonPropertyName("ncCms")]
    public DisbursementType NcCms
    {
      get => ncCms ??= new();
      set => ncCms = value;
    }

    /// <summary>
    /// A value of NcIaj.
    /// </summary>
    [JsonPropertyName("ncIaj")]
    public DisbursementType NcIaj
    {
      get => ncIaj ??= new();
      set => ncIaj = value;
    }

    /// <summary>
    /// A value of NcIms.
    /// </summary>
    [JsonPropertyName("ncIms")]
    public DisbursementType NcIms
    {
      get => ncIms ??= new();
      set => ncIms = value;
    }

    /// <summary>
    /// A value of NcAms.
    /// </summary>
    [JsonPropertyName("ncAms")]
    public DisbursementType NcAms
    {
      get => ncAms ??= new();
      set => ncAms = value;
    }

    /// <summary>
    /// A value of NcIcs.
    /// </summary>
    [JsonPropertyName("ncIcs")]
    public DisbursementType NcIcs
    {
      get => ncIcs ??= new();
      set => ncIcs = value;
    }

    /// <summary>
    /// A value of NcAcs.
    /// </summary>
    [JsonPropertyName("ncAcs")]
    public DisbursementType NcAcs
    {
      get => ncAcs ??= new();
      set => ncAcs = value;
    }

    /// <summary>
    /// A value of NcCcs.
    /// </summary>
    [JsonPropertyName("ncCcs")]
    public DisbursementType NcCcs
    {
      get => ncCcs ??= new();
      set => ncCcs = value;
    }

    /// <summary>
    /// A value of Nc.
    /// </summary>
    [JsonPropertyName("nc")]
    public Collection Nc
    {
      get => nc ??= new();
      set => nc = value;
    }

    /// <summary>
    /// A value of NfCms.
    /// </summary>
    [JsonPropertyName("nfCms")]
    public DisbursementType NfCms
    {
      get => nfCms ??= new();
      set => nfCms = value;
    }

    /// <summary>
    /// A value of NfAmj.
    /// </summary>
    [JsonPropertyName("nfAmj")]
    public DisbursementType NfAmj
    {
      get => nfAmj ??= new();
      set => nfAmj = value;
    }

    /// <summary>
    /// A value of NfImj.
    /// </summary>
    [JsonPropertyName("nfImj")]
    public DisbursementType NfImj
    {
      get => nfImj ??= new();
      set => nfImj = value;
    }

    /// <summary>
    /// A value of NfCmc.
    /// </summary>
    [JsonPropertyName("nfCmc")]
    public DisbursementType NfCmc
    {
      get => nfCmc ??= new();
      set => nfCmc = value;
    }

    /// <summary>
    /// A value of NfAmc.
    /// </summary>
    [JsonPropertyName("nfAmc")]
    public DisbursementType NfAmc
    {
      get => nfAmc ??= new();
      set => nfAmc = value;
    }

    /// <summary>
    /// A value of NfImc.
    /// </summary>
    [JsonPropertyName("nfImc")]
    public DisbursementType NfImc
    {
      get => nfImc ??= new();
      set => nfImc = value;
    }

    /// <summary>
    /// A value of NfAaj.
    /// </summary>
    [JsonPropertyName("nfAaj")]
    public DisbursementType NfAaj
    {
      get => nfAaj ??= new();
      set => nfAaj = value;
    }

    /// <summary>
    /// A value of NfIaj.
    /// </summary>
    [JsonPropertyName("nfIaj")]
    public DisbursementType NfIaj
    {
      get => nfIaj ??= new();
      set => nfIaj = value;
    }

    /// <summary>
    /// A value of NfUme.
    /// </summary>
    [JsonPropertyName("nfUme")]
    public DisbursementType NfUme
    {
      get => nfUme ??= new();
      set => nfUme = value;
    }

    /// <summary>
    /// A value of NfIume1.
    /// </summary>
    [JsonPropertyName("nfIume1")]
    public DisbursementType NfIume1
    {
      get => nfIume1 ??= new();
      set => nfIume1 = value;
    }

    /// <summary>
    /// A value of NfIij.
    /// </summary>
    [JsonPropertyName("nfIij")]
    public DisbursementType NfIij
    {
      get => nfIij ??= new();
      set => nfIij = value;
    }

    /// <summary>
    /// A value of NfAcrch.
    /// </summary>
    [JsonPropertyName("nfAcrch")]
    public DisbursementType NfAcrch
    {
      get => nfAcrch ??= new();
      set => nfAcrch = value;
    }

    /// <summary>
    /// A value of NfIcrch.
    /// </summary>
    [JsonPropertyName("nfIcrch")]
    public DisbursementType NfIcrch
    {
      get => nfIcrch ??= new();
      set => nfIcrch = value;
    }

    /// <summary>
    /// A value of Nf.
    /// </summary>
    [JsonPropertyName("nf")]
    public Collection Nf
    {
      get => nf ??= new();
      set => nf = value;
    }

    /// <summary>
    /// A value of NfCcs.
    /// </summary>
    [JsonPropertyName("nfCcs")]
    public DisbursementType NfCcs
    {
      get => nfCcs ??= new();
      set => nfCcs = value;
    }

    /// <summary>
    /// A value of NfIms.
    /// </summary>
    [JsonPropertyName("nfIms")]
    public DisbursementType NfIms
    {
      get => nfIms ??= new();
      set => nfIms = value;
    }

    /// <summary>
    /// A value of NfAms.
    /// </summary>
    [JsonPropertyName("nfAms")]
    public DisbursementType NfAms
    {
      get => nfAms ??= new();
      set => nfAms = value;
    }

    /// <summary>
    /// A value of NfAcs.
    /// </summary>
    [JsonPropertyName("nfAcs")]
    public DisbursementType NfAcs
    {
      get => nfAcs ??= new();
      set => nfAcs = value;
    }

    /// <summary>
    /// A value of NfIcs.
    /// </summary>
    [JsonPropertyName("nfIcs")]
    public DisbursementType NfIcs
    {
      get => nfIcs ??= new();
      set => nfIcs = value;
    }

    /// <summary>
    /// A value of NfiCms.
    /// </summary>
    [JsonPropertyName("nfiCms")]
    public DisbursementType NfiCms
    {
      get => nfiCms ??= new();
      set => nfiCms = value;
    }

    /// <summary>
    /// A value of NfiAms.
    /// </summary>
    [JsonPropertyName("nfiAms")]
    public DisbursementType NfiAms
    {
      get => nfiAms ??= new();
      set => nfiAms = value;
    }

    /// <summary>
    /// A value of NfiIms.
    /// </summary>
    [JsonPropertyName("nfiIms")]
    public DisbursementType NfiIms
    {
      get => nfiIms ??= new();
      set => nfiIms = value;
    }

    /// <summary>
    /// A value of NfiCmc.
    /// </summary>
    [JsonPropertyName("nfiCmc")]
    public DisbursementType NfiCmc
    {
      get => nfiCmc ??= new();
      set => nfiCmc = value;
    }

    /// <summary>
    /// A value of NfiAmc.
    /// </summary>
    [JsonPropertyName("nfiAmc")]
    public DisbursementType NfiAmc
    {
      get => nfiAmc ??= new();
      set => nfiAmc = value;
    }

    /// <summary>
    /// A value of NfiImc.
    /// </summary>
    [JsonPropertyName("nfiImc")]
    public DisbursementType NfiImc
    {
      get => nfiImc ??= new();
      set => nfiImc = value;
    }

    /// <summary>
    /// A value of NfiArrj.
    /// </summary>
    [JsonPropertyName("nfiArrj")]
    public DisbursementType NfiArrj
    {
      get => nfiArrj ??= new();
      set => nfiArrj = value;
    }

    /// <summary>
    /// A value of NfiIarj.
    /// </summary>
    [JsonPropertyName("nfiIarj")]
    public DisbursementType NfiIarj
    {
      get => nfiIarj ??= new();
      set => nfiIarj = value;
    }

    /// <summary>
    /// A value of NfiUme2.
    /// </summary>
    [JsonPropertyName("nfiUme2")]
    public DisbursementType NfiUme2
    {
      get => nfiUme2 ??= new();
      set => nfiUme2 = value;
    }

    /// <summary>
    /// A value of NfiIume.
    /// </summary>
    [JsonPropertyName("nfiIume")]
    public DisbursementType NfiIume
    {
      get => nfiIume ??= new();
      set => nfiIume = value;
    }

    /// <summary>
    /// A value of NfiIntj.
    /// </summary>
    [JsonPropertyName("nfiIntj")]
    public DisbursementType NfiIntj
    {
      get => nfiIntj ??= new();
      set => nfiIntj = value;
    }

    /// <summary>
    /// A value of NfiCrc.
    /// </summary>
    [JsonPropertyName("nfiCrc")]
    public DisbursementType NfiCrc
    {
      get => nfiCrc ??= new();
      set => nfiCrc = value;
    }

    /// <summary>
    /// A value of NfiIcrc.
    /// </summary>
    [JsonPropertyName("nfiIcrc")]
    public DisbursementType NfiIcrc
    {
      get => nfiIcrc ??= new();
      set => nfiIcrc = value;
    }

    /// <summary>
    /// A value of NfiCcs.
    /// </summary>
    [JsonPropertyName("nfiCcs")]
    public DisbursementType NfiCcs
    {
      get => nfiCcs ??= new();
      set => nfiCcs = value;
    }

    /// <summary>
    /// A value of NfiAcs.
    /// </summary>
    [JsonPropertyName("nfiAcs")]
    public DisbursementType NfiAcs
    {
      get => nfiAcs ??= new();
      set => nfiAcs = value;
    }

    /// <summary>
    /// A value of NfiIcs.
    /// </summary>
    [JsonPropertyName("nfiIcs")]
    public DisbursementType NfiIcs
    {
      get => nfiIcs ??= new();
      set => nfiIcs = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of DispGrp11Xxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("dispGrp11Xxxxxxxxxxx")]
    public Common DispGrp11Xxxxxxxxxxx
    {
      get => dispGrp11Xxxxxxxxxxx ??= new();
      set => dispGrp11Xxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp1Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp1Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp1Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp1Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp1Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of GiftDisbursementType.
    /// </summary>
    [JsonPropertyName("giftDisbursementType")]
    public DisbursementType GiftDisbursementType
    {
      get => giftDisbursementType ??= new();
      set => giftDisbursementType = value;
    }

    /// <summary>
    /// A value of DispGrp2Xxxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("dispGrp2Xxxxxxxxxxxxxxxxxxxxx")]
    public Common DispGrp2Xxxxxxxxxxxxxxxxxxxxx
    {
      get => dispGrp2Xxxxxxxxxxxxxxxxxxxxx ??= new();
      set => dispGrp2Xxxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DispGrp21Xxxxxxx.
    /// </summary>
    [JsonPropertyName("dispGrp21Xxxxxxx")]
    public Common DispGrp21Xxxxxxx
    {
      get => dispGrp21Xxxxxxx ??= new();
      set => dispGrp21Xxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp3Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp3Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp3Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp3Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp3Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp31Xxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp31Xxxxxxxxxxxxxx")]
    public Common DisbGrp31Xxxxxxxxxxxxxx
    {
      get => disbGrp31Xxxxxxxxxxxxxx ??= new();
      set => disbGrp31Xxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp4Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp4Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp4Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp4Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp4Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp41Xxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp41Xxxxxxxxxxxxx")]
    public Common DisbGrp41Xxxxxxxxxxxxx
    {
      get => disbGrp41Xxxxxxxxxxxxx ??= new();
      set => disbGrp41Xxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp5Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp5Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp5Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp5Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp5Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp51Xxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp51Xxxxxxxxxxxxxx")]
    public Common DisbGrp51Xxxxxxxxxxxxxx
    {
      get => disbGrp51Xxxxxxxxxxxxxx ??= new();
      set => disbGrp51Xxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp6Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp6Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp6Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp6Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp6Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp61Xxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp61Xxxxxxxxxx")]
    public Common DisbGrp61Xxxxxxxxxx
    {
      get => disbGrp61Xxxxxxxxxx ??= new();
      set => disbGrp61Xxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp7Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp7Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp7Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp7Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp7Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp71Xxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp71Xxxxxxxxxxx")]
    public Common DisbGrp71Xxxxxxxxxxx
    {
      get => disbGrp71Xxxxxxxxxxx ??= new();
      set => disbGrp71Xxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp8Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp8Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp8Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp8Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp8Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp81Xxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp81Xxxxxxxxxxxxx")]
    public Common DisbGrp81Xxxxxxxxxxxxx
    {
      get => disbGrp81Xxxxxxxxxxxxx ??= new();
      set => disbGrp81Xxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp9Xxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp9Xxxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp9Xxxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp9Xxxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp9Xxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of IvdRcDisbursementType.
    /// </summary>
    [JsonPropertyName("ivdRcDisbursementType")]
    public DisbursementType IvdRcDisbursementType
    {
      get => ivdRcDisbursementType ??= new();
      set => ivdRcDisbursementType = value;
    }

    /// <summary>
    /// A value of IrsNegDisbursementType.
    /// </summary>
    [JsonPropertyName("irsNegDisbursementType")]
    public DisbursementType IrsNegDisbursementType
    {
      get => irsNegDisbursementType ??= new();
      set => irsNegDisbursementType = value;
    }

    /// <summary>
    /// A value of BdckRcDisbursementType.
    /// </summary>
    [JsonPropertyName("bdckRcDisbursementType")]
    public DisbursementType BdckRcDisbursementType
    {
      get => bdckRcDisbursementType ??= new();
      set => bdckRcDisbursementType = value;
    }

    /// <summary>
    /// A value of MisArDisbursementType.
    /// </summary>
    [JsonPropertyName("misArDisbursementType")]
    public DisbursementType MisArDisbursementType
    {
      get => misArDisbursementType ??= new();
      set => misArDisbursementType = value;
    }

    /// <summary>
    /// A value of MisApDisbursementType.
    /// </summary>
    [JsonPropertyName("misApDisbursementType")]
    public DisbursementType MisApDisbursementType
    {
      get => misApDisbursementType ??= new();
      set => misApDisbursementType = value;
    }

    /// <summary>
    /// A value of MisNonDisbursementType.
    /// </summary>
    [JsonPropertyName("misNonDisbursementType")]
    public DisbursementType MisNonDisbursementType
    {
      get => misNonDisbursementType ??= new();
      set => misNonDisbursementType = value;
    }

    /// <summary>
    /// A value of DisbGrp10Xxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp10Xxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp10Xxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp10Xxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp10Xxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp101Xxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp101Xxxxxxxxx")]
    public Common DisbGrp101Xxxxxxxxx
    {
      get => disbGrp101Xxxxxxxxx ??= new();
      set => disbGrp101Xxxxxxxxx = value;
    }

    /// <summary>
    /// A value of DisbGrp11Xxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp11Xxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp11Xxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp11Xxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp11Xxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of ApFee.
    /// </summary>
    [JsonPropertyName("apFee")]
    public DisbursementType ApFee
    {
      get => apFee ??= new();
      set => apFee = value;
    }

    /// <summary>
    /// A value of N718bisbursementType.
    /// </summary>
    [JsonPropertyName("n718bisbursementType")]
    public DisbursementType N718bisbursementType
    {
      get => n718bisbursementType ??= new();
      set => n718bisbursementType = value;
    }

    /// <summary>
    /// A value of I718b.
    /// </summary>
    [JsonPropertyName("i718b")]
    public DisbursementType I718b
    {
      get => i718b ??= new();
      set => i718b = value;
    }

    /// <summary>
    /// A value of DisbGrp12Xxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("disbGrp12Xxxxxxxxxxxxxxxxxxx")]
    public Common DisbGrp12Xxxxxxxxxxxxxxxxxxx
    {
      get => disbGrp12Xxxxxxxxxxxxxxxxxxx ??= new();
      set => disbGrp12Xxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of Pt.
    /// </summary>
    [JsonPropertyName("pt")]
    public DisbursementType Pt
    {
      get => pt ??= new();
      set => pt = value;
    }

    /// <summary>
    /// A value of Ptr.
    /// </summary>
    [JsonPropertyName("ptr")]
    public DisbursementType Ptr
    {
      get => ptr ??= new();
      set => ptr = value;
    }

    /// <summary>
    /// A value of CrFeeDisbursementType.
    /// </summary>
    [JsonPropertyName("crFeeDisbursementType")]
    public DisbursementType CrFeeDisbursementType
    {
      get => crFeeDisbursementType ??= new();
      set => crFeeDisbursementType = value;
    }

    /// <summary>
    /// A value of Coagfee.
    /// </summary>
    [JsonPropertyName("coagfee")]
    public DisbursementType Coagfee
    {
      get => coagfee ??= new();
      set => coagfee = value;
    }

    /// <summary>
    /// A value of ObligGrpZzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("obligGrpZzzzzzzzzzzzzzzzzzzzz")]
    public Common ObligGrpZzzzzzzzzzzzzzzzzzzzz
    {
      get => obligGrpZzzzzzzzzzzzzzzzzzzzz ??= new();
      set => obligGrpZzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Cs.
    /// </summary>
    [JsonPropertyName("cs")]
    public ObligationType Cs
    {
      get => cs ??= new();
      set => cs = value;
    }

    /// <summary>
    /// A value of Sp.
    /// </summary>
    [JsonPropertyName("sp")]
    public ObligationType Sp
    {
      get => sp ??= new();
      set => sp = value;
    }

    /// <summary>
    /// A value of Ms.
    /// </summary>
    [JsonPropertyName("ms")]
    public ObligationType Ms
    {
      get => ms ??= new();
      set => ms = value;
    }

    /// <summary>
    /// A value of Mc.
    /// </summary>
    [JsonPropertyName("mc")]
    public ObligationType Mc
    {
      get => mc ??= new();
      set => mc = value;
    }

    /// <summary>
    /// A value of Arrj.
    /// </summary>
    [JsonPropertyName("arrj")]
    public ObligationType Arrj
    {
      get => arrj ??= new();
      set => arrj = value;
    }

    /// <summary>
    /// A value of Saj.
    /// </summary>
    [JsonPropertyName("saj")]
    public ObligationType Saj
    {
      get => saj ??= new();
      set => saj = value;
    }

    /// <summary>
    /// A value of Mj.
    /// </summary>
    [JsonPropertyName("mj")]
    public ObligationType Mj
    {
      get => mj ??= new();
      set => mj = value;
    }

    /// <summary>
    /// A value of Pume.
    /// </summary>
    [JsonPropertyName("pume")]
    public ObligationType Pume
    {
      get => pume ??= new();
      set => pume = value;
    }

    /// <summary>
    /// A value of Intj.
    /// </summary>
    [JsonPropertyName("intj")]
    public ObligationType Intj
    {
      get => intj ??= new();
      set => intj = value;
    }

    /// <summary>
    /// A value of Crch.
    /// </summary>
    [JsonPropertyName("crch")]
    public ObligationType Crch
    {
      get => crch ??= new();
      set => crch = value;
    }

    /// <summary>
    /// A value of IvdRcObligationType.
    /// </summary>
    [JsonPropertyName("ivdRcObligationType")]
    public ObligationType IvdRcObligationType
    {
      get => ivdRcObligationType ??= new();
      set => ivdRcObligationType = value;
    }

    /// <summary>
    /// A value of IrsNegObligationType.
    /// </summary>
    [JsonPropertyName("irsNegObligationType")]
    public ObligationType IrsNegObligationType
    {
      get => irsNegObligationType ??= new();
      set => irsNegObligationType = value;
    }

    /// <summary>
    /// A value of BdckRcObligationType.
    /// </summary>
    [JsonPropertyName("bdckRcObligationType")]
    public ObligationType BdckRcObligationType
    {
      get => bdckRcObligationType ??= new();
      set => bdckRcObligationType = value;
    }

    /// <summary>
    /// A value of MisArObligationType.
    /// </summary>
    [JsonPropertyName("misArObligationType")]
    public ObligationType MisArObligationType
    {
      get => misArObligationType ??= new();
      set => misArObligationType = value;
    }

    /// <summary>
    /// A value of MisApObligationType.
    /// </summary>
    [JsonPropertyName("misApObligationType")]
    public ObligationType MisApObligationType
    {
      get => misApObligationType ??= new();
      set => misApObligationType = value;
    }

    /// <summary>
    /// A value of MisNonObligationType.
    /// </summary>
    [JsonPropertyName("misNonObligationType")]
    public ObligationType MisNonObligationType
    {
      get => misNonObligationType ??= new();
      set => misNonObligationType = value;
    }

    /// <summary>
    /// A value of Vol.
    /// </summary>
    [JsonPropertyName("vol")]
    public ObligationType Vol
    {
      get => vol ??= new();
      set => vol = value;
    }

    /// <summary>
    /// A value of ApFees.
    /// </summary>
    [JsonPropertyName("apFees")]
    public ObligationType ApFees
    {
      get => apFees ??= new();
      set => apFees = value;
    }

    /// <summary>
    /// A value of N718bbligationType.
    /// </summary>
    [JsonPropertyName("n718bbligationType")]
    public ObligationType N718bbligationType
    {
      get => n718bbligationType ??= new();
      set => n718bbligationType = value;
    }

    /// <summary>
    /// A value of GiftObligationType.
    /// </summary>
    [JsonPropertyName("giftObligationType")]
    public ObligationType GiftObligationType
    {
      get => giftObligationType ??= new();
      set => giftObligationType = value;
    }

    /// <summary>
    /// A value of OtherXxxxxxxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("otherXxxxxxxxxxxxxxxxxxxxxxxxx")]
    public Common OtherXxxxxxxxxxxxxxxxxxxxxxxxx
    {
      get => otherXxxxxxxxxxxxxxxxxxxxxxxxx ??= new();
      set => otherXxxxxxxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of Cash.
    /// </summary>
    [JsonPropertyName("cash")]
    public CashReceiptType Cash
    {
      get => cash ??= new();
      set => cash = value;
    }

    /// <summary>
    /// A value of AppliedToCurrent.
    /// </summary>
    [JsonPropertyName("appliedToCurrent")]
    public Collection AppliedToCurrent
    {
      get => appliedToCurrent ??= new();
      set => appliedToCurrent = value;
    }

    /// <summary>
    /// A value of AppliedToArrears.
    /// </summary>
    [JsonPropertyName("appliedToArrears")]
    public Collection AppliedToArrears
    {
      get => appliedToArrears ??= new();
      set => appliedToArrears = value;
    }

    /// <summary>
    /// A value of AppliedToInterest.
    /// </summary>
    [JsonPropertyName("appliedToInterest")]
    public Collection AppliedToInterest
    {
      get => appliedToInterest ??= new();
      set => appliedToInterest = value;
    }

    /// <summary>
    /// A value of AppliedToGift.
    /// </summary>
    [JsonPropertyName("appliedToGift")]
    public Collection AppliedToGift
    {
      get => appliedToGift ??= new();
      set => appliedToGift = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Collection Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of Afi.
    /// </summary>
    [JsonPropertyName("afi")]
    public Collection Afi
    {
      get => afi ??= new();
      set => afi = value;
    }

    private DisbursementType afGmc;
    private DisbursementType afGcs;
    private DisbursementType afGms;
    private DisbursementType afGsp;
    private DisbursementType afiGmc;
    private DisbursementType afiGcs;
    private DisbursementType afiGms;
    private DisbursementType afiGsp;
    private DisbursementType fcGmc;
    private DisbursementType fcGms;
    private DisbursementType fciGmc;
    private DisbursementType fciGcs;
    private DisbursementType fciGms;
    private DisbursementType naGmc;
    private DisbursementType naGms;
    private DisbursementType naGcs;
    private DisbursementType naGsp;
    private DisbursementType naiGmc;
    private DisbursementType naiGms;
    private DisbursementType naiGcs;
    private DisbursementType naiGsp;
    private DisbursementType ncGmc;
    private DisbursementType ncGcs;
    private DisbursementType ncGms;
    private DisbursementType nfGmc;
    private DisbursementType nfGms;
    private DisbursementType nfGcs;
    private DisbursementType fcGcs;
    private ObligationType crFeeObligationType;
    private DisbursementType afIms;
    private DisbursementType afIsp;
    private DisbursementType afAsp;
    private DisbursementType afAms;
    private DisbursementType afCcs;
    private DisbursementType afAcs;
    private DisbursementType afIcs;
    private DisbursementType afCsp;
    private DisbursementType afAsaj;
    private DisbursementType afIsaj;
    private DisbursementType afCms;
    private DisbursementType afAmj;
    private DisbursementType afImj;
    private DisbursementType afiCms;
    private DisbursementType afCmc;
    private DisbursementType afAmc;
    private DisbursementType afImc;
    private DisbursementType afAaj;
    private DisbursementType afIaj;
    private DisbursementType afUme;
    private DisbursementType afIume1;
    private DisbursementType afIij;
    private DisbursementType afAcrch;
    private DisbursementType afIcrch;
    private DisbursementType afVol;
    private DisbursementType afVolR;
    private DisbursementType afiCcs;
    private DisbursementType afiAcs;
    private DisbursementType afiIcs;
    private DisbursementType afiCsp;
    private DisbursementType afiIms;
    private DisbursementType afiAms;
    private DisbursementType afiIsp;
    private DisbursementType afiAsp;
    private DisbursementType afiAsaj;
    private DisbursementType afiIsaj;
    private DisbursementType afiAmj;
    private DisbursementType afiImj;
    private DisbursementType afiCmc;
    private DisbursementType afiAmc;
    private DisbursementType afiImc;
    private DisbursementType afiAaj;
    private DisbursementType afiIaj;
    private DisbursementType afiUme2;
    private DisbursementType afiIume;
    private DisbursementType afiIij;
    private DisbursementType afiAcrch;
    private DisbursementType afiIcrch;
    private DisbursementType afiVol;
    private DisbursementType afiVolR;
    private Collection fc;
    private DisbursementType fcCcs;
    private DisbursementType fcAcs;
    private DisbursementType fcIcs;
    private DisbursementType fcIms;
    private DisbursementType fcAms;
    private DisbursementType fcCms;
    private DisbursementType fcAmj;
    private DisbursementType fcImj;
    private DisbursementType fcCmc;
    private DisbursementType fcAmc;
    private DisbursementType fcImc;
    private DisbursementType fcAaj;
    private DisbursementType fcIaj;
    private DisbursementType fcUme;
    private DisbursementType fcIume1;
    private DisbursementType fcIij;
    private DisbursementType fcAcrch;
    private DisbursementType fcIcrch;
    private Collection fci;
    private DisbursementType fciCcs;
    private DisbursementType fciAcs;
    private DisbursementType fciIcs;
    private DisbursementType fciCms;
    private DisbursementType fciAmj;
    private DisbursementType fciImj;
    private DisbursementType fciCmc;
    private DisbursementType fciIms;
    private DisbursementType fciAms;
    private DisbursementType fciAmc;
    private DisbursementType fciImc;
    private DisbursementType fciAaj;
    private DisbursementType fciIaj;
    private DisbursementType fciUme2;
    private DisbursementType fciIume;
    private DisbursementType fciIij;
    private DisbursementType fciAcrch;
    private DisbursementType fciIcrch;
    private Collection na;
    private DisbursementType naCcs;
    private DisbursementType naAcs;
    private DisbursementType naIcs;
    private DisbursementType naCcsr;
    private DisbursementType naAcsr;
    private DisbursementType naIcsr;
    private DisbursementType naCsp;
    private DisbursementType naAsaj;
    private DisbursementType naIsaj;
    private DisbursementType naCspr;
    private DisbursementType naAspr;
    private DisbursementType naIspr;
    private DisbursementType naCms;
    private DisbursementType naAmj;
    private DisbursementType naImj;
    private DisbursementType naCmsr;
    private DisbursementType naAmsr;
    private DisbursementType naImsr;
    private DisbursementType naCmc;
    private DisbursementType naAmc;
    private DisbursementType naImc;
    private DisbursementType naCmcr;
    private DisbursementType naAmcr;
    private DisbursementType naImcr;
    private DisbursementType naIsp;
    private DisbursementType naAsp;
    private DisbursementType naAaj;
    private DisbursementType naIarj;
    private DisbursementType naArrjr;
    private DisbursementType naIaj;
    private DisbursementType naUme;
    private DisbursementType naIume1;
    private DisbursementType naUmer;
    private DisbursementType naIms;
    private DisbursementType naAms;
    private DisbursementType naIumer1;
    private DisbursementType naIij;
    private DisbursementType naIntjr;
    private DisbursementType naAcrch;
    private DisbursementType naIcrch;
    private DisbursementType naCrcr;
    private DisbursementType naIcrcr1;
    private DisbursementType naVol;
    private DisbursementType naVolR;
    private DisbursementType naiCcs;
    private DisbursementType naiAcs;
    private DisbursementType naiIms;
    private DisbursementType naiAms;
    private DisbursementType naiIsp;
    private DisbursementType naiAsp;
    private DisbursementType naiIcs;
    private DisbursementType naiCcsr;
    private DisbursementType naiAcsr;
    private DisbursementType naiIcsr;
    private DisbursementType naiCsp;
    private DisbursementType naiAsaj;
    private DisbursementType naiIsaj;
    private DisbursementType naiCspr;
    private DisbursementType naiAspr;
    private DisbursementType naiIspr;
    private DisbursementType naiCms;
    private DisbursementType naiAmj;
    private DisbursementType naiImj;
    private DisbursementType naiCmsr;
    private DisbursementType naiAmsr;
    private DisbursementType naiImsr;
    private DisbursementType naiCmc;
    private DisbursementType naiAmc;
    private DisbursementType naiImc;
    private DisbursementType naiCmcr;
    private DisbursementType naiAmcr;
    private DisbursementType naiImcr;
    private DisbursementType naiAaj;
    private DisbursementType naiIarj;
    private DisbursementType naiArrjr;
    private DisbursementType naiIaj;
    private Collection nai;
    private DisbursementType naiUme2;
    private DisbursementType naiIume;
    private DisbursementType naiUmer2;
    private DisbursementType naiIumer;
    private DisbursementType naiIij;
    private DisbursementType naiIntjr;
    private DisbursementType naiAcrch;
    private DisbursementType naiIcrci;
    private DisbursementType naiCrcr2;
    private DisbursementType naiIcrcr;
    private DisbursementType naiVol;
    private DisbursementType naiVolR;
    private DisbursementType naiIcrch;
    private DisbursementType ncIij;
    private DisbursementType ncAcrch;
    private DisbursementType ncIcrch;
    private DisbursementType ncIume;
    private DisbursementType ncUme;
    private DisbursementType ncIsp;
    private DisbursementType ncAsp;
    private DisbursementType ncIarj;
    private DisbursementType ncAaj;
    private DisbursementType ncImc;
    private DisbursementType ncAmc;
    private DisbursementType ncCmc;
    private DisbursementType ncImj;
    private DisbursementType ncAmj;
    private DisbursementType ncCms;
    private DisbursementType ncIaj;
    private DisbursementType ncIms;
    private DisbursementType ncAms;
    private DisbursementType ncIcs;
    private DisbursementType ncAcs;
    private DisbursementType ncCcs;
    private Collection nc;
    private DisbursementType nfCms;
    private DisbursementType nfAmj;
    private DisbursementType nfImj;
    private DisbursementType nfCmc;
    private DisbursementType nfAmc;
    private DisbursementType nfImc;
    private DisbursementType nfAaj;
    private DisbursementType nfIaj;
    private DisbursementType nfUme;
    private DisbursementType nfIume1;
    private DisbursementType nfIij;
    private DisbursementType nfAcrch;
    private DisbursementType nfIcrch;
    private Collection nf;
    private DisbursementType nfCcs;
    private DisbursementType nfIms;
    private DisbursementType nfAms;
    private DisbursementType nfAcs;
    private DisbursementType nfIcs;
    private DisbursementType nfiCms;
    private DisbursementType nfiAms;
    private DisbursementType nfiIms;
    private DisbursementType nfiCmc;
    private DisbursementType nfiAmc;
    private DisbursementType nfiImc;
    private DisbursementType nfiArrj;
    private DisbursementType nfiIarj;
    private DisbursementType nfiUme2;
    private DisbursementType nfiIume;
    private DisbursementType nfiIntj;
    private DisbursementType nfiCrc;
    private DisbursementType nfiIcrc;
    private DisbursementType nfiCcs;
    private DisbursementType nfiAcs;
    private DisbursementType nfiIcs;
    private DateWorkArea initialized;
    private Common dispGrp11Xxxxxxxxxxx;
    private Common disbGrp1Xxxxxxxxxxxxxxxxxxxx;
    private DisbursementType giftDisbursementType;
    private Common dispGrp2Xxxxxxxxxxxxxxxxxxxxx;
    private Common dispGrp21Xxxxxxx;
    private Common disbGrp3Xxxxxxxxxxxxxxxxxxxx;
    private Common disbGrp31Xxxxxxxxxxxxxx;
    private Common disbGrp4Xxxxxxxxxxxxxxxxxxxx;
    private Common disbGrp41Xxxxxxxxxxxxx;
    private Common disbGrp5Xxxxxxxxxxxxxxxxxxxx;
    private Common disbGrp51Xxxxxxxxxxxxxx;
    private Common disbGrp6Xxxxxxxxxxxxxxxxxxxx;
    private Common disbGrp61Xxxxxxxxxx;
    private Common disbGrp7Xxxxxxxxxxxxxxxxxxxx;
    private Common disbGrp71Xxxxxxxxxxx;
    private Common disbGrp8Xxxxxxxxxxxxxxxxxxxx;
    private Common disbGrp81Xxxxxxxxxxxxx;
    private Common disbGrp9Xxxxxxxxxxxxxxxxxxxx;
    private DisbursementType ivdRcDisbursementType;
    private DisbursementType irsNegDisbursementType;
    private DisbursementType bdckRcDisbursementType;
    private DisbursementType misArDisbursementType;
    private DisbursementType misApDisbursementType;
    private DisbursementType misNonDisbursementType;
    private Common disbGrp10Xxxxxxxxxxxxxxxxxxx;
    private Common disbGrp101Xxxxxxxxx;
    private Common disbGrp11Xxxxxxxxxxxxxxxxxxx;
    private DisbursementType apFee;
    private DisbursementType n718bisbursementType;
    private DisbursementType i718b;
    private Common disbGrp12Xxxxxxxxxxxxxxxxxxx;
    private DisbursementType pt;
    private DisbursementType ptr;
    private DisbursementType crFeeDisbursementType;
    private DisbursementType coagfee;
    private Common obligGrpZzzzzzzzzzzzzzzzzzzzz;
    private ObligationType cs;
    private ObligationType sp;
    private ObligationType ms;
    private ObligationType mc;
    private ObligationType arrj;
    private ObligationType saj;
    private ObligationType mj;
    private ObligationType pume;
    private ObligationType intj;
    private ObligationType crch;
    private ObligationType ivdRcObligationType;
    private ObligationType irsNegObligationType;
    private ObligationType bdckRcObligationType;
    private ObligationType misArObligationType;
    private ObligationType misApObligationType;
    private ObligationType misNonObligationType;
    private ObligationType vol;
    private ObligationType apFees;
    private ObligationType n718bbligationType;
    private ObligationType giftObligationType;
    private Common otherXxxxxxxxxxxxxxxxxxxxxxxxx;
    private CashReceiptType cash;
    private Collection appliedToCurrent;
    private Collection appliedToArrears;
    private Collection appliedToInterest;
    private Collection appliedToGift;
    private Collection af;
    private Collection afi;
  }
#endregion
}
