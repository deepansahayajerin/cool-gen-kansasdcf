// Program: SI_FORMAT_CSENET_OUT_TRANS, ID: 372618194, model: 746.
// Short name: SWE02322
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_FORMAT_CSENET_OUT_TRANS.
/// </summary>
[Serializable]
public partial class SiFormatCsenetOutTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FORMAT_CSENET_OUT_TRANS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFormatCsenetOutTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFormatCsenetOutTrans.
  /// </summary>
  public SiFormatCsenetOutTrans(IContext context, Import import, Export export):
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
    // 02/12/1999	Carl Ott	Initial Dev.
    // 07/30/1999	M Ramirez	Added local view for
    //                                 
    // Action_reason_code second byte
    //                                 
    // (successful/unsuccessful)
    // 07/30/1999	M Ramirez	Modified checks for multiple
    //                                 
    // datablocks for Participant,
    // Order
    //                                 
    // and Collection to verify
    // 				exact matching from what is
    //                                 
    // found on the DATAbase and what
    // is found on the
    // 				interstate_case header information.
    // 08/06/1999	M Ramirez	Added call to validate
    // 				Function - Action - Reason
    //                                 
    // combination.
    // 02/02/2000      C Scroggins     Added code to fill mandatory
    //                                 
    // data requirements for outgoing
    //                                 
    // CSENet transactions.
    // 04/27/2001	M Ramirez	WR 287 - CSI can be sent without AP Id
    // 
    // 01/24/2007      A Hockman      Fix to bump START date one day LESS than
    // start
    //                                  
    // date if both dates are equal  PR
    // 287643
    // ------------------------------------------------------------
    local.MaxDate.Date = new DateTime(2099, 12, 31);

    if (ReadInterstateCase())
    {
      export.InterstateCase.Assign(entities.InterstateCase);

      // cls
      // -------------------------------------------------
      // 02/02/2000
      // Added code to check for empty attachment indicator, and, if so, to 
      // populate it.
      // --------------------------------------------------------------
      if (IsEmpty(export.InterstateCase.AttachmentsInd))
      {
        export.InterstateCase.AttachmentsInd = "N";
      }

      // cls
      // -------------------------------------------------
      // 02/03/2000
      // Added code to check max date value and, if so, change it to min date 
      // value.
      // --------------------------------------------------------------
      if (Equal(export.InterstateCase.TransactionDate, local.MaxDate.Date))
      {
        export.InterstateCase.TransactionDate = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.ActionResolutionDate, local.MaxDate.Date))
      {
        export.InterstateCase.ActionResolutionDate = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.SentDate, local.MaxDate.Date))
      {
        export.InterstateCase.SentDate = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.AttachmentsDueDate, local.MaxDate.Date))
      {
        export.InterstateCase.AttachmentsDueDate = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.AssnDeactDt, local.MaxDate.Date))
      {
        export.InterstateCase.AssnDeactDt = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.LastDeferDt, local.MaxDate.Date))
      {
        export.InterstateCase.LastDeferDt = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.DueDate, local.MaxDate.Date))
      {
        export.InterstateCase.DueDate = local.MinDate.Date;
      }

      if (Equal(export.InterstateCase.DateReceived, local.MaxDate.Date))
      {
        export.InterstateCase.DateReceived = local.MinDate.Date;
      }

      local.ActRsnCodeSuccessCode.Flag =
        Substring(entities.InterstateCase.ActionReasonCode, 2, 1);

      // mjr
      // -------------------------------------------------
      // 08/06/1999
      // Added call to validate Function - Action - Reason combination
      // --------------------------------------------------------------
      UseSiValidateCsenetTransType();

      if (!IsEmpty(local.Error.Text10))
      {
        export.WriteError.Flag = "Y";
        export.NeededToWrite.RptDetail = "Invalid CSENet transaction type (" + TrimEnd
          (local.Error.Text10) + "):  " + export
          .InterstateCase.FunctionalTypeCode + ", " + export
          .InterstateCase.ActionCode + ", " + (
            export.InterstateCase.ActionReasonCode ?? "");
        export.NeededToWrite.RptDetail =
          TrimEnd(export.NeededToWrite.RptDetail) + "       " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

        return;
      }

      // **************************************************************
      // Convert Case Type codes from KESSEP values to CSENet values.
      // **************************************************************
      switch(TrimEnd(entities.InterstateCase.CaseType))
      {
        case "CC":
          export.InterstateCase.CaseType = "N";

          break;
        case "AF":
          export.InterstateCase.CaseType = "A";

          break;
        case "NF":
          export.InterstateCase.CaseType = "F";

          break;
        case "FS":
          export.InterstateCase.CaseType = "N";

          break;
        case "MA":
          export.InterstateCase.CaseType = "N";

          break;
        case "MS":
          export.InterstateCase.CaseType = "N";

          break;
        case "CI":
          export.InterstateCase.CaseType = "N";

          break;
        case "SI":
          export.InterstateCase.CaseType = "N";

          break;
        case "MP":
          export.InterstateCase.CaseType = "N";

          break;
        case "MK":
          export.InterstateCase.CaseType = "N";

          break;
        case "NA":
          export.InterstateCase.CaseType = "N";

          break;
        case "FC":
          export.InterstateCase.CaseType = "F";

          break;
        case "NC":
          export.InterstateCase.CaseType = "N";

          break;
        default:
          break;
      }

      if (Equal(entities.InterstateCase.ApIdentificationInd, 1))
      {
        if (ReadInterstateApIdentification())
        {
          export.InterstateApIdentification.Assign(
            entities.InterstateApIdentification);

          // cls
          // -------------------------------------------------
          // 02/03/2000
          // Added code to check max date value and, if so, change it to min 
          // date value.
          // --------------------------------------------------------------
          if (Equal(export.InterstateApIdentification.DateOfBirth,
            local.MaxDate.Date))
          {
            export.InterstateApIdentification.DateOfBirth = local.MinDate.Date;
          }

          // **************************************************************
          // Convert Race codes from KESSEP values to CSENet values.
          // **************************************************************
          switch(TrimEnd(entities.InterstateApIdentification.Race))
          {
            case "AI":
              export.InterstateApIdentification.Race = "I";

              break;
            case "AJ":
              export.InterstateApIdentification.Race = "I";

              break;
            case "BL":
              export.InterstateApIdentification.Race = "B";

              break;
            case "HI":
              export.InterstateApIdentification.Race = "S";

              break;
            case "HP":
              export.InterstateApIdentification.Race = "A";

              break;
            case "OT":
              export.InterstateApIdentification.Race = "X";

              break;
            case "SA":
              export.InterstateApIdentification.Race = "A";

              break;
            case "UK":
              export.InterstateApIdentification.Race = "X";

              break;
            case "WH":
              export.InterstateApIdentification.Race = "W";

              break;
            default:
              export.InterstateApIdentification.Race = "X";

              break;
          }

          if (Equal(entities.InterstateCase.ApLocateDataInd, 1))
          {
            if (ReadInterstateApLocate())
            {
              export.InterstateApLocate.Assign(entities.InterstateApLocate);

              // cls
              // -------------------------------------------------
              // 02/03/2000
              // Added code to check for max date and if so, change it to min 
              // date.
              // --------------------------------------------------------------
              if (Equal(export.InterstateApLocate.Employer2EffectiveDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.Employer2EffectiveDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.Employer2EndDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.Employer2EndDate = local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.Employer3EffectiveDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.Employer3EffectiveDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.Employer3EndDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.Employer3EndDate = local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.EmployerEffectiveDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.EmployerEffectiveDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.EmployerEndDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.EmployerEndDate = local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.LastEmployerDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.LastEmployerDate = local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.LastEmployerEndDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.LastEmployerEndDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.LastMailAddressDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.LastMailAddressDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.LastResAddressDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.LastResAddressDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.MailingAddressEffectiveDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.MailingAddressEffectiveDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.MailingAddressEndDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.MailingAddressEndDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.ResidentialAddressEffectvDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.ResidentialAddressEffectvDate =
                  local.MinDate.Date;
              }

              if (Equal(export.InterstateApLocate.ResidentialAddressEndDate,
                local.MaxDate.Date))
              {
                export.InterstateApLocate.ResidentialAddressEndDate =
                  local.MinDate.Date;
              }
            }
            else
            {
              export.WriteError.Flag = "Y";
            }
          }
          else if (Equal(entities.InterstateCase.ApLocateDataInd, 0))
          {
            if (AsChar(entities.InterstateCase.ActionCode) == 'R' || AsChar
              (entities.InterstateCase.ActionCode) == 'U')
            {
              if (Equal(entities.InterstateCase.FunctionalTypeCode, "PAT") || Equal
                (entities.InterstateCase.FunctionalTypeCode, "EST") || Equal
                (entities.InterstateCase.FunctionalTypeCode, "ENF"))
              {
                // ***************************************************************
                // AP Locate data block is required for these combinations of 
                // action code and functional type code.
                // ***************************************************************
                export.WriteError.Flag = "Y";
              }
            }
            else if (AsChar(entities.InterstateCase.ActionCode) == 'P')
            {
              if ((Equal(entities.InterstateCase.FunctionalTypeCode, "LO1") || Equal
                (entities.InterstateCase.FunctionalTypeCode, "LO2")) && AsChar
                (local.ActRsnCodeSuccessCode.Flag) == 'S')
              {
                // ***************************************************************
                // AP Locate data block is required for these combinations of 
                // action code and functional type code.
                // ***************************************************************
                export.WriteError.Flag = "Y";
              }
            }
          }
          else
          {
            // ***************************************************************
            // Invalid value for AP Locate Ind.  Must be 0 or 1.
            // ***************************************************************
            export.WriteError.Flag = "Y";
            export.NeededToWrite.RptDetail = "AP Locate Ind. value '" + NumberToString
              (entities.InterstateCase.ApLocateDataInd.GetValueOrDefault(), 15,
              1) + "' invalid, must be '0' or '1'.";
            export.NeededToWrite.RptDetail =
              TrimEnd(export.NeededToWrite.RptDetail) + "       " + NumberToString
              (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
              (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

            return;
          }

          if (AsChar(export.WriteError.Flag) == 'Y')
          {
            export.NeededToWrite.RptDetail =
              "AP Locate record not found                                  " + NumberToString
              (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
              (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

            return;
          }
        }
        else
        {
          export.WriteError.Flag = "Y";
        }
      }
      else if (Equal(entities.InterstateCase.ApIdentificationInd, 0))
      {
        switch(TrimEnd(entities.InterstateCase.FunctionalTypeCode))
        {
          case "COL":
            break;
          case "CSI":
            // mjr
            // --------------------------------------------------
            // 04/27/2001
            // WR 287 - CSI can be sent without AP Id
            // ---------------------------------------------------------------
            break;
          case "MSC":
            break;
          default:
            // ****************************************************************
            // An AP Identification Data Block is required for all Functional 
            // Type Codes except COL and MSC.
            // ****************************************************************
            export.WriteError.Flag = "Y";

            break;
        }
      }
      else
      {
        // ***************************************************************
        // Invalid value for AP Identification Ind.  Must be 0 or 1.
        // ***************************************************************
        export.WriteError.Flag = "Y";
        export.NeededToWrite.RptDetail = "AP Ident Ind. value '" + NumberToString
          (entities.InterstateCase.ApIdentificationInd.GetValueOrDefault(), 15,
          1) + "' invalid, must be '0' or '1'.       ";
        export.NeededToWrite.RptDetail =
          TrimEnd(export.NeededToWrite.RptDetail) + "       " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

        return;
      }

      if (AsChar(export.WriteError.Flag) == 'Y')
      {
        export.NeededToWrite.RptDetail =
          "AP Ident record not found                                   " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

        return;
      }

      if (Lt(0, entities.InterstateCase.ParticipantDataInd))
      {
        export.Participant.Index = 0;
        export.Participant.Clear();

        foreach(var item in ReadInterstateParticipant())
        {
          export.Participant.Update.InterstateParticipant.Assign(
            entities.InterstateParticipant);

          // cls
          // -------------------------------------------------
          // 02/03/2000
          // Added code to check for max date and if so, change it to min date.
          // --------------------------------------------------------------
          if (Equal(export.Participant.Item.InterstateParticipant.
            AddressVerifiedDate, local.MaxDate.Date))
          {
            export.Participant.Update.InterstateParticipant.
              AddressVerifiedDate = local.MinDate.Date;
          }

          if (Equal(export.Participant.Item.InterstateParticipant.DateOfBirth,
            local.MaxDate.Date))
          {
            export.Participant.Update.InterstateParticipant.DateOfBirth =
              local.MinDate.Date;
          }

          if (Equal(export.Participant.Item.InterstateParticipant.
            EmployerVerifiedDate, local.MaxDate.Date))
          {
            export.Participant.Update.InterstateParticipant.
              EmployerVerifiedDate = local.MinDate.Date;
          }

          // **************************************************************
          // Convert Race codes from KESSEP values to CSENet values.
          // **************************************************************
          switch(TrimEnd(entities.InterstateParticipant.Race))
          {
            case "AI":
              export.Participant.Update.InterstateParticipant.Race = "I";

              break;
            case "AJ":
              export.Participant.Update.InterstateParticipant.Race = "I";

              break;
            case "BL":
              export.Participant.Update.InterstateParticipant.Race = "B";

              break;
            case "HI":
              export.Participant.Update.InterstateParticipant.Race = "S";

              break;
            case "HP":
              export.Participant.Update.InterstateParticipant.Race = "A";

              break;
            case "OT":
              export.Participant.Update.InterstateParticipant.Race = "X";

              break;
            case "SA":
              export.Participant.Update.InterstateParticipant.Race = "A";

              break;
            case "UK":
              export.Participant.Update.InterstateParticipant.Race = "X";

              break;
            case "WH":
              export.Participant.Update.InterstateParticipant.Race = "W";

              break;
            default:
              export.Participant.Update.InterstateParticipant.Race = "X";

              break;
          }

          switch(TrimEnd(entities.InterstateParticipant.Relationship))
          {
            case "AP":
              export.Participant.Update.InterstateParticipant.Relationship =
                "A";

              break;
            case "AR":
              export.Participant.Update.InterstateParticipant.Relationship =
                "C";

              break;
            case "CH":
              export.Participant.Update.InterstateParticipant.Relationship =
                "D";

              break;
            default:
              break;
          }

          switch(TrimEnd(entities.InterstateParticipant.DependentRelationCp))
          {
            case "CO":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "U";

              break;
            case "FC":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "F";

              break;
            case "CH":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "C";

              break;
            case "GC":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "G";

              break;
            case "NN":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "E";

              break;
            case "NR":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "N";

              break;
            case "OR":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "O";

              break;
            case "SB":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "S";

              break;
            case "SC":
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "O";

              break;
            default:
              export.Participant.Update.InterstateParticipant.
                DependentRelationCp = "O";

              break;
          }

          switch(AsChar(entities.InterstateParticipant.Status))
          {
            case 'A':
              export.Participant.Update.InterstateParticipant.Status = "O";

              break;
            case 'I':
              export.Participant.Update.InterstateParticipant.Status = "C";

              break;
            default:
              break;
          }

          ++local.CsenetPartDataBlock.Count;

          if (Equal(entities.InterstateParticipant.Relationship, "AR"))
          {
            ++local.CpPart.Count;
          }

          if (Equal(entities.InterstateParticipant.Relationship, "CH"))
          {
            ++local.DepPart.Count;
          }

          export.Participant.Next();
        }

        if (!Equal(local.CsenetPartDataBlock.Count,
          entities.InterstateCase.ParticipantDataInd))
        {
          export.WriteError.Flag = "Y";

          goto Test;
        }

        if (AsChar(entities.InterstateCase.ActionCode) == 'R' || AsChar
          (entities.InterstateCase.ActionCode) == 'U')
        {
          if (Equal(entities.InterstateCase.FunctionalTypeCode, "PAT") || Equal
            (entities.InterstateCase.FunctionalTypeCode, "EST") || Equal
            (entities.InterstateCase.FunctionalTypeCode, "ENF") || Equal
            (entities.InterstateCase.FunctionalTypeCode, "LO2"))
          {
            if (local.CpPart.Count < 1)
            {
              // ***************************************************************
              // A Participant data block for custodial parent is required for 
              // these combinations of action code and functional type code.
              // ***************************************************************
              export.NeededToWrite.RptDetail =
                "Participant record not found for Applicant-Recipient        " +
                NumberToString
                (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
                (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
              export.WriteError.Flag = "Y";

              return;
            }

            if (local.DepPart.Count < 1)
            {
              // ***************************************************************
              // A Participant data block for dependent child is required for 
              // these combinations of action code and functional type code.
              // ***************************************************************
              export.NeededToWrite.RptDetail =
                "Participant record not found for Child                      " +
                NumberToString
                (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
                (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
              export.WriteError.Flag = "Y";

              return;
            }
          }
        }
      }
      else if (AsChar(entities.InterstateCase.ActionCode) == 'R' || AsChar
        (entities.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(entities.InterstateCase.FunctionalTypeCode, "PAT") || Equal
          (entities.InterstateCase.FunctionalTypeCode, "EST") || Equal
          (entities.InterstateCase.FunctionalTypeCode, "ENF") || Equal
          (entities.InterstateCase.FunctionalTypeCode, "LO2"))
        {
          // ***************************************************************
          // A Participant data block for custodial parent and dependent child 
          // are required for these combinations of action code and functional
          // type code.
          // ***************************************************************
          export.WriteError.Flag = "Y";
        }
      }

Test:

      if (AsChar(export.WriteError.Flag) == 'Y')
      {
        export.NeededToWrite.RptDetail =
          "Participant record not found                                " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

        return;
      }

      if (Lt(0, entities.InterstateCase.OrderDataInd))
      {
        export.Order.Index = 0;
        export.Order.Clear();

        foreach(var item in ReadInterstateSupportOrder())
        {
          export.Order.Update.InterstateSupportOrder.Assign(
            entities.InterstateSupportOrder);

          // cls
          // -------------------------------------------------
          // 02/03/2000
          // Added code to check for max date and if so, change it to min date.
          // --------------------------------------------------------------
          if (Equal(export.Order.Item.InterstateSupportOrder.
            ArrearsAfdcFromDate, local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.ArrearsAfdcFromDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.
            ArrearsAfdcThruDate, local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.ArrearsAfdcThruDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.
            ArrearsNonAfdcFromDate, local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.ArrearsNonAfdcFromDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.
            ArrearsNonAfdcThruDate, local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.ArrearsNonAfdcThruDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.CancelDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.CancelDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.DateOfLastPayment,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.DateOfLastPayment =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.EffectiveDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.EffectiveDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.EndDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.EndDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.FosterCareFromDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.FosterCareFromDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.FosterCareThruDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.FosterCareThruDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.MedicalFromDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.MedicalFromDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.MedicalThruDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.MedicalThruDate =
              local.MinDate.Date;
          }

          if (Equal(export.Order.Item.InterstateSupportOrder.OrderFilingDate,
            local.MaxDate.Date))
          {
            export.Order.Update.InterstateSupportOrder.OrderFilingDate =
              local.MinDate.Date;
          }

          // ***  pr 287643 - - - - - - - - - - - 
          // -------------------------------------------
          // 01/24/2007
          //  added qualifiers for from_date equal to  thru_date and bump 
          // from_date back one day
          // since Feds no longer allow these dates to be equal.  Not making any
          // actual data base changes,
          // just changing this on the outgoing transactions to keep them from 
          // erroring.     Anita Hockman
          // ------------------------------------------------------------------------------
          if (!Equal(export.Order.Item.InterstateSupportOrder.
            ArrearsNonAfdcThruDate, local.MinDate.Date))
          {
            if (Equal(export.Order.Item.InterstateSupportOrder.
              ArrearsNonAfdcFromDate,
              export.Order.Item.InterstateSupportOrder.ArrearsNonAfdcThruDate))
            {
              export.Order.Update.InterstateSupportOrder.
                ArrearsNonAfdcFromDate =
                  AddDays(export.Order.Item.InterstateSupportOrder.
                  ArrearsNonAfdcFromDate, -1);
            }
          }

          if (!Equal(export.Order.Item.InterstateSupportOrder.
            ArrearsAfdcThruDate, local.MinDate.Date))
          {
            if (Equal(export.Order.Item.InterstateSupportOrder.
              ArrearsAfdcFromDate,
              export.Order.Item.InterstateSupportOrder.ArrearsAfdcThruDate))
            {
              export.Order.Update.InterstateSupportOrder.ArrearsAfdcFromDate =
                AddDays(export.Order.Item.InterstateSupportOrder.
                  ArrearsAfdcFromDate, -1);
            }
          }

          if (!Equal(export.Order.Item.InterstateSupportOrder.
            FosterCareThruDate, local.MinDate.Date))
          {
            if (Equal(export.Order.Item.InterstateSupportOrder.
              FosterCareFromDate,
              export.Order.Item.InterstateSupportOrder.FosterCareThruDate))
            {
              export.Order.Update.InterstateSupportOrder.FosterCareFromDate =
                AddDays(export.Order.Item.InterstateSupportOrder.
                  FosterCareFromDate, -1);
            }
          }

          if (!Equal(export.Order.Item.InterstateSupportOrder.MedicalThruDate,
            local.MinDate.Date))
          {
            if (Equal(export.Order.Item.InterstateSupportOrder.MedicalFromDate,
              export.Order.Item.InterstateSupportOrder.MedicalThruDate))
            {
              export.Order.Update.InterstateSupportOrder.MedicalFromDate =
                AddDays(export.Order.Item.InterstateSupportOrder.
                  MedicalFromDate, -1);
            }
          }

          switch(TrimEnd(entities.InterstateSupportOrder.DebtType))
          {
            case "CRCH":
              export.Order.Update.InterstateSupportOrder.DebtType = "CS";

              break;
            case "AJ":
              export.Order.Update.InterstateSupportOrder.DebtType = "CS";

              break;
            case "MC":
              export.Order.Update.InterstateSupportOrder.DebtType = "MS";

              break;
            case "MJ":
              export.Order.Update.InterstateSupportOrder.DebtType = "MS";

              break;
            case "SAJ":
              export.Order.Update.InterstateSupportOrder.DebtType = "SS";

              break;
            case "SP":
              export.Order.Update.InterstateSupportOrder.DebtType = "SS";

              break;
            case "718B":
              export.Order.Update.InterstateSupportOrder.DebtType = "CS";

              break;
            case "HIC":
              export.Order.Update.InterstateSupportOrder.DebtType = "MS";

              break;
            case "UM":
              export.Order.Update.InterstateSupportOrder.DebtType = "MS";

              break;
            default:
              break;
          }

          switch(TrimEnd(entities.InterstateSupportOrder.PaymentFreq))
          {
            case "BW":
              export.Order.Update.InterstateSupportOrder.PaymentFreq = "B";

              break;
            case "SM":
              export.Order.Update.InterstateSupportOrder.PaymentFreq = "S";

              break;
            default:
              break;
          }

          ++local.CsenetOrderDataBlock.Count;
          export.Order.Next();
        }

        if (!Equal(local.CsenetOrderDataBlock.Count,
          entities.InterstateCase.OrderDataInd))
        {
          export.WriteError.Flag = "Y";
        }
      }
      else if (AsChar(entities.InterstateCase.ActionCode) == 'R' || AsChar
        (entities.InterstateCase.ActionCode) == 'U')
      {
        if (Equal(entities.InterstateCase.FunctionalTypeCode, "ENF"))
        {
          // ***************************************************************
          // An Order data block is required for these combinations of action 
          // code and functional type code.
          // ***************************************************************
          export.WriteError.Flag = "Y";
        }
      }

      if (AsChar(export.WriteError.Flag) == 'Y')
      {
        export.NeededToWrite.RptDetail =
          "Support Order record not found                              " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

        return;
      }

      if (Lt(0, entities.InterstateCase.CollectionDataInd))
      {
        export.Collection.Index = 0;
        export.Collection.Clear();

        foreach(var item in ReadInterstateCollection())
        {
          switch(AsChar(entities.InterstateCollection.PaymentSource))
          {
            case 'F':
              export.Collection.Update.InterstateCollection.Assign(
                entities.InterstateCollection);

              // cls
              // -------------------------------------------------
              // 02/03/2000
              // Added code to check for max date and if so, change it to min 
              // date.
              // --------------------------------------------------------------
              if (Equal(export.Collection.Item.InterstateCollection.
                DateOfPosting, local.MaxDate.Date))
              {
                export.Collection.Update.InterstateCollection.DateOfPosting =
                  local.MinDate.Date;
              }

              if (Equal(export.Collection.Item.InterstateCollection.
                DateOfCollection, local.MaxDate.Date))
              {
                export.Collection.Update.InterstateCollection.DateOfCollection =
                  local.MinDate.Date;
              }

              export.Collection.Update.InterstateCollection.PaymentSource = "I";

              break;
            case 'S':
              export.Collection.Update.InterstateCollection.Assign(
                entities.InterstateCollection);

              // cls
              // -------------------------------------------------
              // 02/03/2000
              // Added code to check for max date and if so, change it to min 
              // date.
              // --------------------------------------------------------------
              if (Equal(export.Collection.Item.InterstateCollection.
                DateOfPosting, local.MaxDate.Date))
              {
                export.Collection.Update.InterstateCollection.DateOfPosting =
                  local.MinDate.Date;
              }

              if (Equal(export.Collection.Item.InterstateCollection.
                DateOfCollection, local.MaxDate.Date))
              {
                export.Collection.Update.InterstateCollection.DateOfCollection =
                  local.MinDate.Date;
              }

              break;
            default:
              export.Collection.Next();

              continue;
          }

          ++local.CsenetCollectDataBlock.Count;
          export.Collection.Next();
        }

        if (!Equal(local.CsenetCollectDataBlock.Count,
          entities.InterstateCase.CollectionDataInd))
        {
          export.WriteError.Flag = "Y";
        }
      }
      else if (AsChar(entities.InterstateCase.ActionCode) == 'P')
      {
        if (Equal(entities.InterstateCase.FunctionalTypeCode, "COL"))
        {
          // ***************************************************************
          // A Collection data block is required for these combinations of 
          // action code and functional type code.
          // ***************************************************************
          export.WriteError.Flag = "Y";
        }
      }

      if (AsChar(export.WriteError.Flag) == 'Y')
      {
        export.NeededToWrite.RptDetail =
          "Collection record not found                                 " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);

        return;
      }

      if (Equal(entities.InterstateCase.InformationInd, 1) || AsChar
        (entities.InterstateCase.AttachmentsInd) == 'Y')
      {
        if (ReadInterstateMiscellaneous())
        {
          export.InterstateMiscellaneous.
            Assign(entities.InterstateMiscellaneous);
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.NeededToWrite.RptDetail =
            "Information record not found                                " + NumberToString
            (entities.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
        }
      }
    }
    else
    {
      ExitState = "CSENET_CASE_NF";
    }
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.ActionReasonCode = source.ActionReasonCode;
  }

  private void UseSiValidateCsenetTransType()
  {
    var useImport = new SiValidateCsenetTransType.Import();
    var useExport = new SiValidateCsenetTransType.Export();

    MoveInterstateCase(export.InterstateCase, useImport.InterstateCase);

    Call(SiValidateCsenetTransType.Execute, useImport, useExport);

    local.Error.Text10 = useExport.Error.Text10;
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 7);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 14);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 16);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 17);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.PossiblyDangerous =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.MaidenName =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.MothersMaidenOrFathersName =
          db.GetNullableString(reader, 21);
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateApLocate()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateApIdentification.Populated);
    entities.InterstateApLocate.Populated = false;

    return Read("ReadInterstateApLocate",
      (db, command) =>
      {
        db.SetInt64(
          command, "cncTransSerlNbr",
          entities.InterstateApIdentification.CcaTransSerNum);
        db.SetDate(
          command, "cncTransactionDt",
          entities.InterstateApIdentification.CcaTransactionDt.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApLocate.CncTransactionDt = db.GetDate(reader, 0);
        entities.InterstateApLocate.CncTransSerlNbr = db.GetInt64(reader, 1);
        entities.InterstateApLocate.EmployerEin =
          db.GetNullableInt32(reader, 2);
        entities.InterstateApLocate.EmployerName =
          db.GetNullableString(reader, 3);
        entities.InterstateApLocate.EmployerPhoneNum =
          db.GetNullableInt32(reader, 4);
        entities.InterstateApLocate.EmployerEffectiveDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateApLocate.EmployerEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstateApLocate.EmployerConfirmedInd =
          db.GetNullableString(reader, 7);
        entities.InterstateApLocate.ResidentialAddressLine1 =
          db.GetNullableString(reader, 8);
        entities.InterstateApLocate.ResidentialAddressLine2 =
          db.GetNullableString(reader, 9);
        entities.InterstateApLocate.ResidentialCity =
          db.GetNullableString(reader, 10);
        entities.InterstateApLocate.ResidentialState =
          db.GetNullableString(reader, 11);
        entities.InterstateApLocate.ResidentialZipCode5 =
          db.GetNullableString(reader, 12);
        entities.InterstateApLocate.ResidentialZipCode4 =
          db.GetNullableString(reader, 13);
        entities.InterstateApLocate.MailingAddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateApLocate.MailingAddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateApLocate.MailingCity =
          db.GetNullableString(reader, 16);
        entities.InterstateApLocate.MailingState =
          db.GetNullableString(reader, 17);
        entities.InterstateApLocate.MailingZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateApLocate.MailingZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateApLocate.ResidentialAddressEffectvDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateApLocate.ResidentialAddressEndDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateApLocate.ResidentialAddressConfirmInd =
          db.GetNullableString(reader, 22);
        entities.InterstateApLocate.MailingAddressEffectiveDate =
          db.GetNullableDate(reader, 23);
        entities.InterstateApLocate.MailingAddressEndDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateApLocate.MailingAddressConfirmedInd =
          db.GetNullableString(reader, 25);
        entities.InterstateApLocate.HomePhoneNumber =
          db.GetNullableInt32(reader, 26);
        entities.InterstateApLocate.WorkPhoneNumber =
          db.GetNullableInt32(reader, 27);
        entities.InterstateApLocate.DriversLicState =
          db.GetNullableString(reader, 28);
        entities.InterstateApLocate.DriversLicenseNum =
          db.GetNullableString(reader, 29);
        entities.InterstateApLocate.Alias1FirstName =
          db.GetNullableString(reader, 30);
        entities.InterstateApLocate.Alias1MiddleName =
          db.GetNullableString(reader, 31);
        entities.InterstateApLocate.Alias1LastName =
          db.GetNullableString(reader, 32);
        entities.InterstateApLocate.Alias1Suffix =
          db.GetNullableString(reader, 33);
        entities.InterstateApLocate.Alias2FirstName =
          db.GetNullableString(reader, 34);
        entities.InterstateApLocate.Alias2MiddleName =
          db.GetNullableString(reader, 35);
        entities.InterstateApLocate.Alias2LastName =
          db.GetNullableString(reader, 36);
        entities.InterstateApLocate.Alias2Suffix =
          db.GetNullableString(reader, 37);
        entities.InterstateApLocate.Alias3FirstName =
          db.GetNullableString(reader, 38);
        entities.InterstateApLocate.Alias3MiddleName =
          db.GetNullableString(reader, 39);
        entities.InterstateApLocate.Alias3LastName =
          db.GetNullableString(reader, 40);
        entities.InterstateApLocate.Alias3Suffix =
          db.GetNullableString(reader, 41);
        entities.InterstateApLocate.CurrentSpouseFirstName =
          db.GetNullableString(reader, 42);
        entities.InterstateApLocate.CurrentSpouseMiddleName =
          db.GetNullableString(reader, 43);
        entities.InterstateApLocate.CurrentSpouseLastName =
          db.GetNullableString(reader, 44);
        entities.InterstateApLocate.CurrentSpouseSuffix =
          db.GetNullableString(reader, 45);
        entities.InterstateApLocate.Occupation =
          db.GetNullableString(reader, 46);
        entities.InterstateApLocate.EmployerAddressLine1 =
          db.GetNullableString(reader, 47);
        entities.InterstateApLocate.EmployerAddressLine2 =
          db.GetNullableString(reader, 48);
        entities.InterstateApLocate.EmployerCity =
          db.GetNullableString(reader, 49);
        entities.InterstateApLocate.EmployerState =
          db.GetNullableString(reader, 50);
        entities.InterstateApLocate.EmployerZipCode5 =
          db.GetNullableString(reader, 51);
        entities.InterstateApLocate.EmployerZipCode4 =
          db.GetNullableString(reader, 52);
        entities.InterstateApLocate.WageQtr = db.GetNullableInt32(reader, 53);
        entities.InterstateApLocate.WageYear = db.GetNullableInt32(reader, 54);
        entities.InterstateApLocate.WageAmount =
          db.GetNullableDecimal(reader, 55);
        entities.InterstateApLocate.InsuranceCarrierName =
          db.GetNullableString(reader, 56);
        entities.InterstateApLocate.InsurancePolicyNum =
          db.GetNullableString(reader, 57);
        entities.InterstateApLocate.LastResAddressLine1 =
          db.GetNullableString(reader, 58);
        entities.InterstateApLocate.LastResAddressLine2 =
          db.GetNullableString(reader, 59);
        entities.InterstateApLocate.LastResCity =
          db.GetNullableString(reader, 60);
        entities.InterstateApLocate.LastResState =
          db.GetNullableString(reader, 61);
        entities.InterstateApLocate.LastResZipCode5 =
          db.GetNullableString(reader, 62);
        entities.InterstateApLocate.LastResZipCode4 =
          db.GetNullableString(reader, 63);
        entities.InterstateApLocate.LastResAddressDate =
          db.GetNullableDate(reader, 64);
        entities.InterstateApLocate.LastMailAddressLine1 =
          db.GetNullableString(reader, 65);
        entities.InterstateApLocate.LastMailAddressLine2 =
          db.GetNullableString(reader, 66);
        entities.InterstateApLocate.LastMailCity =
          db.GetNullableString(reader, 67);
        entities.InterstateApLocate.LastMailState =
          db.GetNullableString(reader, 68);
        entities.InterstateApLocate.LastMailZipCode5 =
          db.GetNullableString(reader, 69);
        entities.InterstateApLocate.LastMailZipCode4 =
          db.GetNullableString(reader, 70);
        entities.InterstateApLocate.LastMailAddressDate =
          db.GetNullableDate(reader, 71);
        entities.InterstateApLocate.LastEmployerName =
          db.GetNullableString(reader, 72);
        entities.InterstateApLocate.LastEmployerDate =
          db.GetNullableDate(reader, 73);
        entities.InterstateApLocate.LastEmployerAddressLine1 =
          db.GetNullableString(reader, 74);
        entities.InterstateApLocate.LastEmployerAddressLine2 =
          db.GetNullableString(reader, 75);
        entities.InterstateApLocate.LastEmployerCity =
          db.GetNullableString(reader, 76);
        entities.InterstateApLocate.LastEmployerState =
          db.GetNullableString(reader, 77);
        entities.InterstateApLocate.LastEmployerZipCode5 =
          db.GetNullableString(reader, 78);
        entities.InterstateApLocate.LastEmployerZipCode4 =
          db.GetNullableString(reader, 79);
        entities.InterstateApLocate.ProfessionalLicenses =
          db.GetNullableString(reader, 80);
        entities.InterstateApLocate.WorkAreaCode =
          db.GetNullableInt32(reader, 81);
        entities.InterstateApLocate.HomeAreaCode =
          db.GetNullableInt32(reader, 82);
        entities.InterstateApLocate.LastEmployerEndDate =
          db.GetNullableDate(reader, 83);
        entities.InterstateApLocate.EmployerAreaCode =
          db.GetNullableInt32(reader, 84);
        entities.InterstateApLocate.Employer2Name =
          db.GetNullableString(reader, 85);
        entities.InterstateApLocate.Employer2Ein =
          db.GetNullableInt32(reader, 86);
        entities.InterstateApLocate.Employer2PhoneNumber =
          db.GetNullableString(reader, 87);
        entities.InterstateApLocate.Employer2AreaCode =
          db.GetNullableString(reader, 88);
        entities.InterstateApLocate.Employer2AddressLine1 =
          db.GetNullableString(reader, 89);
        entities.InterstateApLocate.Employer2AddressLine2 =
          db.GetNullableString(reader, 90);
        entities.InterstateApLocate.Employer2City =
          db.GetNullableString(reader, 91);
        entities.InterstateApLocate.Employer2State =
          db.GetNullableString(reader, 92);
        entities.InterstateApLocate.Employer2ZipCode5 =
          db.GetNullableInt32(reader, 93);
        entities.InterstateApLocate.Employer2ZipCode4 =
          db.GetNullableInt32(reader, 94);
        entities.InterstateApLocate.Employer2ConfirmedIndicator =
          db.GetNullableString(reader, 95);
        entities.InterstateApLocate.Employer2EffectiveDate =
          db.GetNullableDate(reader, 96);
        entities.InterstateApLocate.Employer2EndDate =
          db.GetNullableDate(reader, 97);
        entities.InterstateApLocate.Employer2WageAmount =
          db.GetNullableInt64(reader, 98);
        entities.InterstateApLocate.Employer2WageQuarter =
          db.GetNullableInt32(reader, 99);
        entities.InterstateApLocate.Employer2WageYear =
          db.GetNullableInt32(reader, 100);
        entities.InterstateApLocate.Employer3Name =
          db.GetNullableString(reader, 101);
        entities.InterstateApLocate.Employer3Ein =
          db.GetNullableInt32(reader, 102);
        entities.InterstateApLocate.Employer3PhoneNumber =
          db.GetNullableString(reader, 103);
        entities.InterstateApLocate.Employer3AreaCode =
          db.GetNullableString(reader, 104);
        entities.InterstateApLocate.Employer3AddressLine1 =
          db.GetNullableString(reader, 105);
        entities.InterstateApLocate.Employer3AddressLine2 =
          db.GetNullableString(reader, 106);
        entities.InterstateApLocate.Employer3City =
          db.GetNullableString(reader, 107);
        entities.InterstateApLocate.Employer3State =
          db.GetNullableString(reader, 108);
        entities.InterstateApLocate.Employer3ZipCode5 =
          db.GetNullableInt32(reader, 109);
        entities.InterstateApLocate.Employer3ZipCode4 =
          db.GetNullableInt32(reader, 110);
        entities.InterstateApLocate.Employer3ConfirmedIndicator =
          db.GetNullableString(reader, 111);
        entities.InterstateApLocate.Employer3EffectiveDate =
          db.GetNullableDate(reader, 112);
        entities.InterstateApLocate.Employer3EndDate =
          db.GetNullableDate(reader, 113);
        entities.InterstateApLocate.Employer3WageAmount =
          db.GetNullableInt64(reader, 114);
        entities.InterstateApLocate.Employer3WageQuarter =
          db.GetNullableInt32(reader, 115);
        entities.InterstateApLocate.Employer3WageYear =
          db.GetNullableInt32(reader, 116);
        entities.InterstateApLocate.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 6);
        entities.InterstateCase.ActionCode = db.GetString(reader, 7);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 10);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 14);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 15);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 22);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 23);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 24);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 26);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.InterstateCase.CaseType = db.GetString(reader, 30);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 34);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 35);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 40);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 41);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 42);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 48);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 49);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 50);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 51);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 55);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 56);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 58);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 59);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 60);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 62);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 66);
        entities.InterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCollection()
  {
    return ReadEach("ReadInterstateCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        if (export.Collection.IsFull)
        {
          return false;
        }

        entities.InterstateCollection.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateCollection.CcaTransSerNum = db.GetInt64(reader, 1);
        entities.InterstateCollection.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 2);
        entities.InterstateCollection.DateOfCollection =
          db.GetNullableDate(reader, 3);
        entities.InterstateCollection.DateOfPosting =
          db.GetNullableDate(reader, 4);
        entities.InterstateCollection.PaymentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.InterstateCollection.PaymentSource =
          db.GetNullableString(reader, 6);
        entities.InterstateCollection.InterstatePaymentMethod =
          db.GetNullableString(reader, 7);
        entities.InterstateCollection.RdfiId = db.GetNullableString(reader, 8);
        entities.InterstateCollection.RdfiAccountNum =
          db.GetNullableString(reader, 9);
        entities.InterstateCollection.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateMiscellaneous()
  {
    entities.InterstateMiscellaneous.Populated = false;

    return Read("ReadInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.InterstateMiscellaneous.StatusChangeCode =
          db.GetString(reader, 0);
        entities.InterstateMiscellaneous.NewCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateMiscellaneous.InformationTextLine1 =
          db.GetNullableString(reader, 2);
        entities.InterstateMiscellaneous.InformationTextLine2 =
          db.GetNullableString(reader, 3);
        entities.InterstateMiscellaneous.InformationTextLine3 =
          db.GetNullableString(reader, 4);
        entities.InterstateMiscellaneous.CcaTransSerNum =
          db.GetInt64(reader, 5);
        entities.InterstateMiscellaneous.CcaTransactionDt =
          db.GetDate(reader, 6);
        entities.InterstateMiscellaneous.InformationTextLine4 =
          db.GetNullableString(reader, 7);
        entities.InterstateMiscellaneous.InformationTextLine5 =
          db.GetNullableString(reader, 8);
        entities.InterstateMiscellaneous.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateParticipant()
  {
    return ReadEach("ReadInterstateParticipant",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        if (export.Participant.IsFull)
        {
          return false;
        }

        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.NameLast =
          db.GetNullableString(reader, 3);
        entities.InterstateParticipant.NameFirst =
          db.GetNullableString(reader, 4);
        entities.InterstateParticipant.NameMiddle =
          db.GetNullableString(reader, 5);
        entities.InterstateParticipant.NameSuffix =
          db.GetNullableString(reader, 6);
        entities.InterstateParticipant.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.InterstateParticipant.Ssn = db.GetNullableString(reader, 8);
        entities.InterstateParticipant.Sex = db.GetNullableString(reader, 9);
        entities.InterstateParticipant.Race = db.GetNullableString(reader, 10);
        entities.InterstateParticipant.Relationship =
          db.GetNullableString(reader, 11);
        entities.InterstateParticipant.Status =
          db.GetNullableString(reader, 12);
        entities.InterstateParticipant.DependentRelationCp =
          db.GetNullableString(reader, 13);
        entities.InterstateParticipant.AddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateParticipant.AddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateParticipant.City = db.GetNullableString(reader, 16);
        entities.InterstateParticipant.State = db.GetNullableString(reader, 17);
        entities.InterstateParticipant.ZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateParticipant.ZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateParticipant.EmployerAddressLine1 =
          db.GetNullableString(reader, 20);
        entities.InterstateParticipant.EmployerAddressLine2 =
          db.GetNullableString(reader, 21);
        entities.InterstateParticipant.EmployerCity =
          db.GetNullableString(reader, 22);
        entities.InterstateParticipant.EmployerState =
          db.GetNullableString(reader, 23);
        entities.InterstateParticipant.EmployerZipCode5 =
          db.GetNullableString(reader, 24);
        entities.InterstateParticipant.EmployerZipCode4 =
          db.GetNullableString(reader, 25);
        entities.InterstateParticipant.EmployerName =
          db.GetNullableString(reader, 26);
        entities.InterstateParticipant.EmployerEin =
          db.GetNullableInt32(reader, 27);
        entities.InterstateParticipant.AddressVerifiedDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateParticipant.EmployerVerifiedDate =
          db.GetNullableDate(reader, 29);
        entities.InterstateParticipant.WorkPhone =
          db.GetNullableString(reader, 30);
        entities.InterstateParticipant.WorkAreaCode =
          db.GetNullableString(reader, 31);
        entities.InterstateParticipant.PlaceOfBirth =
          db.GetNullableString(reader, 32);
        entities.InterstateParticipant.ChildStateOfResidence =
          db.GetNullableString(reader, 33);
        entities.InterstateParticipant.ChildPaternityStatus =
          db.GetNullableString(reader, 34);
        entities.InterstateParticipant.EmployerConfirmedInd =
          db.GetNullableString(reader, 35);
        entities.InterstateParticipant.AddressConfirmedInd =
          db.GetNullableString(reader, 36);
        entities.InterstateParticipant.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateSupportOrder()
  {
    return ReadEach("ReadInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTranSerNum", entities.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        if (export.Order.IsFull)
        {
          return false;
        }

        entities.InterstateSupportOrder.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateSupportOrder.CcaTranSerNum = db.GetInt64(reader, 2);
        entities.InterstateSupportOrder.FipsState = db.GetString(reader, 3);
        entities.InterstateSupportOrder.FipsCounty =
          db.GetNullableString(reader, 4);
        entities.InterstateSupportOrder.FipsLocation =
          db.GetNullableString(reader, 5);
        entities.InterstateSupportOrder.Number = db.GetString(reader, 6);
        entities.InterstateSupportOrder.OrderFilingDate = db.GetDate(reader, 7);
        entities.InterstateSupportOrder.Type1 = db.GetNullableString(reader, 8);
        entities.InterstateSupportOrder.DebtType = db.GetString(reader, 9);
        entities.InterstateSupportOrder.PaymentFreq =
          db.GetNullableString(reader, 10);
        entities.InterstateSupportOrder.AmountOrdered =
          db.GetNullableDecimal(reader, 11);
        entities.InterstateSupportOrder.EffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.InterstateSupportOrder.EndDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateSupportOrder.CancelDate =
          db.GetNullableDate(reader, 14);
        entities.InterstateSupportOrder.ArrearsFreq =
          db.GetNullableString(reader, 15);
        entities.InterstateSupportOrder.ArrearsFreqAmount =
          db.GetNullableDecimal(reader, 16);
        entities.InterstateSupportOrder.ArrearsTotalAmount =
          db.GetNullableDecimal(reader, 17);
        entities.InterstateSupportOrder.ArrearsAfdcFromDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateSupportOrder.ArrearsAfdcThruDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateSupportOrder.ArrearsAfdcAmount =
          db.GetNullableDecimal(reader, 20);
        entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateSupportOrder.ArrearsNonAfdcAmount =
          db.GetNullableDecimal(reader, 23);
        entities.InterstateSupportOrder.FosterCareFromDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateSupportOrder.FosterCareThruDate =
          db.GetNullableDate(reader, 25);
        entities.InterstateSupportOrder.FosterCareAmount =
          db.GetNullableDecimal(reader, 26);
        entities.InterstateSupportOrder.MedicalFromDate =
          db.GetNullableDate(reader, 27);
        entities.InterstateSupportOrder.MedicalThruDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateSupportOrder.MedicalAmount =
          db.GetNullableDecimal(reader, 29);
        entities.InterstateSupportOrder.MedicalOrderedInd =
          db.GetNullableString(reader, 30);
        entities.InterstateSupportOrder.TribunalCaseNumber =
          db.GetNullableString(reader, 31);
        entities.InterstateSupportOrder.DateOfLastPayment =
          db.GetNullableDate(reader, 32);
        entities.InterstateSupportOrder.ControllingOrderFlag =
          db.GetNullableString(reader, 33);
        entities.InterstateSupportOrder.NewOrderFlag =
          db.GetNullableString(reader, 34);
        entities.InterstateSupportOrder.DocketNumber =
          db.GetNullableString(reader, 35);
        entities.InterstateSupportOrder.Populated = true;

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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of InterstateParticipant.
      /// </summary>
      [JsonPropertyName("interstateParticipant")]
      public InterstateParticipant InterstateParticipant
      {
        get => interstateParticipant ??= new();
        set => interstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant interstateParticipant;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    private InterstateCase interstateCase;
    private EabReportSend neededToWrite;
    private Common writeError;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private Array<ParticipantGroup> participant;
    private Array<OrderGroup> order;
    private Array<CollectionGroup> collection;
    private InterstateMiscellaneous interstateMiscellaneous;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MinDate.
    /// </summary>
    [JsonPropertyName("minDate")]
    public DateWorkArea MinDate
    {
      get => minDate ??= new();
      set => minDate = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public WorkArea Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ActRsnCodeSuccessCode.
    /// </summary>
    [JsonPropertyName("actRsnCodeSuccessCode")]
    public Common ActRsnCodeSuccessCode
    {
      get => actRsnCodeSuccessCode ??= new();
      set => actRsnCodeSuccessCode = value;
    }

    /// <summary>
    /// A value of DepPart.
    /// </summary>
    [JsonPropertyName("depPart")]
    public Common DepPart
    {
      get => depPart ??= new();
      set => depPart = value;
    }

    /// <summary>
    /// A value of CpPart.
    /// </summary>
    [JsonPropertyName("cpPart")]
    public Common CpPart
    {
      get => cpPart ??= new();
      set => cpPart = value;
    }

    /// <summary>
    /// A value of CsenetCollectDataBlock.
    /// </summary>
    [JsonPropertyName("csenetCollectDataBlock")]
    public Common CsenetCollectDataBlock
    {
      get => csenetCollectDataBlock ??= new();
      set => csenetCollectDataBlock = value;
    }

    /// <summary>
    /// A value of CsenetOrderDataBlock.
    /// </summary>
    [JsonPropertyName("csenetOrderDataBlock")]
    public Common CsenetOrderDataBlock
    {
      get => csenetOrderDataBlock ??= new();
      set => csenetOrderDataBlock = value;
    }

    /// <summary>
    /// A value of CsenetPartDataBlock.
    /// </summary>
    [JsonPropertyName("csenetPartDataBlock")]
    public Common CsenetPartDataBlock
    {
      get => csenetPartDataBlock ??= new();
      set => csenetPartDataBlock = value;
    }

    private DateWorkArea minDate;
    private DateWorkArea maxDate;
    private WorkArea error;
    private Common actRsnCodeSuccessCode;
    private Common depPart;
    private Common cpPart;
    private Common csenetCollectDataBlock;
    private Common csenetOrderDataBlock;
    private Common csenetPartDataBlock;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateCollection interstateCollection;
    private InterstateSupportOrder interstateSupportOrder;
    private InterstateParticipant interstateParticipant;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
  }
#endregion
}
