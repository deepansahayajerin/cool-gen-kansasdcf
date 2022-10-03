// Program: FN_B691_WRITE_RECS_TYPE_3_4_5_6, ID: 371026269, model: 746.
// Short name: SWE00372
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B691_WRITE_RECS_TYPE_3_4_5_6.
/// </summary>
[Serializable]
public partial class FnB691WriteRecsType3456: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B691_WRITE_RECS_TYPE_3_4_5_6 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB691WriteRecsType3456(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB691WriteRecsType3456.
  /// </summary>
  public FnB691WriteRecsType3456(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ============================================================================
    // Written      10/18/2000  V. Madhira
    // PR # 113796  02/22/2001  E. Lyman   Fixed read on CP.  See note in the 
    // code.
    //                                     
    // Abend if Adabas unavailable.
    // CQ# 9690     12/13/2010  J. Huss    Set FVI to spaces since it is now 
    // sent
    // 				    as part of the FVI extract (SWEFB603/SRRUN248)
    // ============================================================================
    local.FileNumber.Count = 1;
    local.Send.Parm1 = "GR";
    export.TotalWritten.Count = import.TotalWritten.Count;

    // *****************************************************************************
    // Read Each Obligor (account type = R)
    // *****************************************************************************
    foreach(var item in ReadCsePersonLegalActionPerson2())
    {
      MoveCsePerson(entities.Ncp, local.HoldNcp);

      // ****************************
      // Set NCP Debt Information
      // ****************************
      local.NcpDebtRecord.RecordType = "3";
      local.ObligorCount.Count = 0;

      // *****************************************************************************
      // Read Each Individual (person type = C for Client)
      // *****************************************************************************
      foreach(var item1 in ReadCsePerson4())
      {
        if (Equal(entities.CsePerson.Number, local.HoldNcp.Number))
        {
        }
        else
        {
          ++local.ObligorCount.Count;

          // =================================================================================
          // This specific court_order has multiple obligors associated to it. 
          // So this is a multipayor situation.
          // =================================================================================
          break;
        }
      }

      if (local.ObligorCount.Count >= 1)
      {
        local.CsePersonsWorkSet.Number = entities.Ncp.Number;

        // ************************************************
        // *Call EAB to retrieve information about a CSE  *
        // *PERSON from the ADABAS system.                *
        // ************************************************
        UseCabReadAdabasPersonBatch1();

        if (Equal(local.AbendData.AdabasResponseCd, "0148"))
        {
          UseFnB691PrintErrorLine();
          ExitState = "ADABAS_UNAVAILABLE_RB";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseFnB691PrintErrorLine();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          local.ErrorFound.Flag = "Y";
        }

        if (AsChar(local.CsePersonsWorkSet.Sex) == 'M')
        {
          local.NcpDebtRecord.KessepMultiplePayerIndicator = "F";
        }
        else
        {
          local.NcpDebtRecord.KessepMultiplePayerIndicator = "M";
        }
      }
      else
      {
        local.NcpDebtRecord.KessepMultiplePayerIndicator = "";
      }

      if (ReadObligationType())
      {
        local.Hold.Assign(entities.ObligationType);

        if (Equal(entities.ObligationType.Code, "CS") || Equal
          (entities.ObligationType.Code, "AJ"))
        {
          local.NcpDebtRecord.DebtType = "CS";
        }
        else if (Equal(entities.ObligationType.Code, "SP") || Equal
          (entities.ObligationType.Code, "SAJ"))
        {
          local.NcpDebtRecord.DebtType = "MN";
        }
        else
        {
          local.NcpDebtRecord.DebtType = "OT";
        }
      }
      else
      {
        ExitState = "OBLIGATION_TYPE_NF";
        UseFnB691PrintErrorLine();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.ErrorFound.Flag = "Y";
      }

      local.NcpDebtRecord.FeeClass = "I";
      local.NcpDebtRecord.OverrideFeePercent = "00.00";
      local.NcpDebtRecord.KpcDebtId = "00000000";

      // ******************************************************
      // Write NCP Debt Record to Output File
      // ******************************************************
      local.PrintFileRecord.CourtOrderLine = "";
      local.PrintFileRecord.CourtOrderLine = local.NcpDebtRecord.RecordType + local
        .NcpDebtRecord.CourtDebtId + local.NcpDebtRecord.DebtType + local
        .NcpDebtRecord.FeeClass + local.NcpDebtRecord.OverrideFeePercent + local
        .NcpDebtRecord.DebtFeeExemption + local.NcpDebtRecord.IntersateId + local
        .NcpDebtRecord.KessepMultiplePayerIndicator + local
        .NcpDebtRecord.CountyMultiplePayorIndicator + local
        .NcpDebtRecord.KpcDebtId + local.NcpDebtRecord.Filler;
      UseFnExtWriteInterfaceFile();
      ++export.TotalWritten.Count;

      if (!IsEmpty(local.Return1.Parm1))
      {
        ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

        return;
      }

      // *******************************************************
      // Set Obligation Information
      // *******************************************************
      local.ObligationRecord.RecordType = "4";

      if (AsChar(local.Hold.Classification) == 'A')
      {
        local.ObligationRecord.Frequency =
          import.LegalActionDetail.FreqPeriodCode ?? Spaces(12);
        local.ObligationRecord.NewAmount =
          import.LegalActionDetail.CurrentAmount.GetValueOrDefault();
        local.ObligationRecord.Amount =
          NumberToString((long)local.ObligationRecord.NewAmount, 9, 7) + TrimEnd
          (".") + NumberToString
          ((long)(local.ObligationRecord.NewAmount * 100), 14, 2);
      }
      else
      {
        local.ObligationRecord.Frequency = "A";
        local.ObligationRecord.Amount = "0000000.00";
      }

      local.ObligationRecord.StartDate =
        NumberToString(DateToInt(import.LegalActionDetail.EffectiveDate), 8, 8);
        
      local.ObligationRecord.EndDate = "99991231";
      local.ObligationRecord.SeasonalFlag = "";

      // ******************************************************
      // Write Obligation Record to Output File
      // ******************************************************
      local.PrintFileRecord.CourtOrderLine = "";
      local.PrintFileRecord.CourtOrderLine =
        local.ObligationRecord.RecordType + local.ObligationRecord.Amount + local
        .ObligationRecord.Frequency + local.ObligationRecord.StartDate + local
        .ObligationRecord.EndDate + local.ObligationRecord.SeasonalFlag + local
        .ObligationRecord.Filler;
      UseFnExtWriteInterfaceFile();
      ++export.TotalWritten.Count;

      if (!IsEmpty(local.Return1.Parm1))
      {
        ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

        return;
      }

      // ***********************
      // Get NCP information.
      // ***********************
      // ********************************************************
      // The person type determines if the obligor is a person
      // or an organization.
      // ********************************************************
      if (AsChar(entities.Ncp.Type1) == 'C')
      {
        local.NcpParticipantRecord.RecordType = "5";
        local.NcpParticipantRecord.Type1 = "I";
        local.CsePersonsWorkSet.Number = entities.Ncp.Number;

        // ************************************************
        // *Call EAB to retrieve information about a CSE  *
        // *PERSON from the ADABAS system.                *
        // ************************************************
        UseCabReadAdabasPersonBatch2();

        if (Equal(local.AbendData.AdabasResponseCd, "0148"))
        {
          UseFnB691PrintErrorLine();
          ExitState = "ADABAS_UNAVAILABLE_RB";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseFnB691PrintErrorLine();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          local.ErrorFound.Flag = "Y";
        }

        local.NcpParticipantRecord.Role = "NCP";
        local.NcpParticipantRecord.FirstName =
          local.CsePersonsWorkSet.FirstName;
        local.NcpParticipantRecord.MiddleInitial =
          local.CsePersonsWorkSet.MiddleInitial;
        local.NcpParticipantRecord.LastName = local.CsePersonsWorkSet.LastName;
        local.NcpParticipantRecord.Ssn = local.CsePersonsWorkSet.Ssn;
        local.NcpParticipantRecord.Gender = local.CsePersonsWorkSet.Sex;
        local.NcpParticipantRecord.SrsPersonNumber = entities.Ncp.Number;
        local.NcpParticipantRecord.FamilyViolenceIndicator = "";
        local.NcpParticipantRecord.DateOfBirth =
          NumberToString(DateToInt(local.CsePersonsWorkSet.Dob), 8, 8);
        local.NcpParticipantRecord.Pin = "00000000";
        local.NcpParticipantRecord.Source = "SRS";

        // ******************************************************
        // Write NCP Participant Record to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(local.NcpParticipantRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            local.NcpParticipantRecord.RecordType + local
            .NcpParticipantRecord.Role + local.NcpParticipantRecord.Type1 + local
            .NcpParticipantRecord.Ssn + local.NcpParticipantRecord.LastName + local
            .NcpParticipantRecord.FirstName + local
            .NcpParticipantRecord.MiddleInitial + local
            .NcpParticipantRecord.Suffix + local.NcpParticipantRecord.Gender + local
            .NcpParticipantRecord.DateOfBirth + local
            .NcpParticipantRecord.SrsPersonNumber + local
            .NcpParticipantRecord.FamilyViolenceIndicator + local
            .NcpParticipantRecord.Pin + local.NcpParticipantRecord.Source + local
            .NcpParticipantRecord.Filler;
          UseFnExtWriteInterfaceFile();
          ++export.TotalWritten.Count;

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }

        // ********************
        // Get NCP address
        // ********************
        if (ReadCsePersonAddress3())
        {
          local.NcpAddressRecord.RecordType = "6";
          local.NcpAddressRecord.Type1 = "M";
          local.NcpAddressRecord.Source = "SRS";
          local.NcpAddressRecord.City = entities.CsePersonAddress.City ?? Spaces
            (20);
          local.NcpAddressRecord.Street = entities.CsePersonAddress.Street1 ?? Spaces
            (30);
          local.NcpAddressRecord.Street2 =
            entities.CsePersonAddress.Street2 ?? Spaces(30);

          if (AsChar(entities.CsePersonAddress.LocationType) == 'D')
          {
            local.NcpAddressRecord.Province = "";
            local.NcpAddressRecord.Country = "";
            local.NcpAddressRecord.State = entities.CsePersonAddress.State ?? Spaces
              (2);

            if (IsEmpty(entities.CsePersonAddress.Zip4))
            {
              local.NcpAddressRecord.PostalCode =
                entities.CsePersonAddress.ZipCode ?? Spaces(10);
            }
            else
            {
              local.NcpAddressRecord.PostalCode =
                entities.CsePersonAddress.ZipCode + "-" + entities
                .CsePersonAddress.Zip4;
            }
          }
          else if (AsChar(entities.CsePersonAddress.LocationType) == 'F')
          {
            local.NcpAddressRecord.Province =
              entities.CsePersonAddress.Province ?? Spaces(5);
            local.NcpAddressRecord.Country =
              entities.CsePersonAddress.Country ?? Spaces(20);
            local.NcpAddressRecord.State = "";
            local.NcpAddressRecord.PostalCode =
              entities.CsePersonAddress.PostalCode ?? Spaces(10);
          }
        }

        // ******************************************************
        // Write NCP Address Records to Output File
        // ******************************************************
        local.PrintFileRecord.CourtOrderLine = "";

        if (!IsEmpty(local.NcpAddressRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine =
            local.NcpAddressRecord.RecordType + local
            .NcpAddressRecord.Street + local.NcpAddressRecord.Street2 + local
            .NcpAddressRecord.City + local.NcpAddressRecord.State + local
            .NcpAddressRecord.PostalCode + local.NcpAddressRecord.Country + local
            .NcpAddressRecord.PhoneNumber + local.NcpAddressRecord.Province + local
            .NcpAddressRecord.Source + local.NcpAddressRecord.Type1 + local
            .NcpAddressRecord.Filler;
          UseFnExtWriteInterfaceFile();
          ++export.TotalWritten.Count;

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }
      }
      else if (AsChar(entities.Ncp.Type1) == 'O')
      {
        // -------------------------------------------------------------------------
        // An organization can not be an obligor. There is some problem with 
        // data. write an error to the file.
        // -------------------------------------------------------------------------
        ExitState = "FN0000_ORGZ_CANNOT_BE_OBLIGOR";
        UseFnB691PrintErrorLine();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.ErrorFound.Flag = "Y";

        continue;
      }

      // ********************************************************
      // Code to get the CP information based on  NCP role.
      // ********************************************************
      if (ReadLegalActionPerson())
      {
        if (AsChar(entities.NcpRole.Role) == 'P')
        {
          // ***********************************************************
          // If the non custodial parent (obligor) is the petitioner...
          // ***********************************************************
          if (ReadCsePerson3())
          {
            // ********************************************************
            // The person type determines if the obligor is a person
            // or an organization.
            // ********************************************************
            if (AsChar(entities.CpCsePerson.Type1) == 'C')
            {
              local.CpParticipantRecord.RecordType = "5";
              local.CpParticipantRecord.Type1 = "I";
              local.CsePersonsWorkSet.Number = entities.CpCsePerson.Number;

              // ************************************************
              // *Call EAB to retrieve information about a CSE  *
              // *PERSON from the ADABAS system.                *
              // ************************************************
              UseCabReadAdabasPersonBatch2();

              if (Equal(local.AbendData.AdabasResponseCd, "0148"))
              {
                UseFnB691PrintErrorLine();
                ExitState = "ADABAS_UNAVAILABLE_RB";

                return;
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseFnB691PrintErrorLine();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.ErrorFound.Flag = "Y";
              }

              local.CpParticipantRecord.Role = "CP";
              local.CpParticipantRecord.FirstName =
                local.CsePersonsWorkSet.FirstName;
              local.CpParticipantRecord.MiddleInitial =
                local.CsePersonsWorkSet.MiddleInitial;
              local.CpParticipantRecord.LastName =
                local.CsePersonsWorkSet.LastName;
              local.CpParticipantRecord.Ssn = local.CsePersonsWorkSet.Ssn;
              local.CpParticipantRecord.Gender = local.CsePersonsWorkSet.Sex;
              local.CpParticipantRecord.SrsPersonNumber =
                entities.CpCsePerson.Number;
              local.CpParticipantRecord.FamilyViolenceIndicator = "";
              local.CpParticipantRecord.DateOfBirth =
                NumberToString(DateToInt(local.CsePersonsWorkSet.Dob), 8, 8);
              local.CpParticipantRecord.Pin = "00000000";
              local.CpParticipantRecord.Source = "SRS";
            }
            else if (AsChar(entities.CpCsePerson.Type1) == 'O')
            {
              local.CpThirdPartyRecord.RecordType = "5";
              local.CpThirdPartyRecord.Type1 = "A";
              local.CpThirdPartyRecord.Role = "TPP";
              local.CpThirdPartyRecord.AgencyName = "SRS";
              local.CpThirdPartyRecord.SrsPersonNumber = "";
              local.CpThirdPartyRecord.Pin = "";
              local.CpThirdPartyRecord.Source = "SRS";
            }
          }
          else
          {
            // ******************************************
            // If no CP is found, set the TPP to SRS
            // ******************************************
            local.CpThirdPartyRecord.RecordType = "5";
            local.CpThirdPartyRecord.Type1 = "A";
            local.CpThirdPartyRecord.Role = "TPP";
            local.CpThirdPartyRecord.AgencyName = "SRS";
            local.CpThirdPartyRecord.SrsPersonNumber = "";
            local.CpThirdPartyRecord.Pin = "";
            local.CpThirdPartyRecord.Source = "SRS";
          }
        }
        else if (AsChar(entities.NcpRole.Role) == 'R')
        {
          // ***********************************************************
          // If the non custodial parent (obligor) is the respondent...
          // ***********************************************************
          if (ReadCsePersonLegalActionPerson1())
          {
            local.CpCsePersonFound.Flag = "N";

            if (AsChar(entities.CpCsePerson.Type1) == 'O')
            {
              // -----------------------------------------------------------------------------
              // If the Custodial Parent is an organization, find the case
              // for this legal action, then determine the current AR.
              // Although originally set up with the organization as the CP,
              // later a parent may have been awarded custody.  This
              // will allow the KPC to send checks to the new CP.  PR113796
              // ----------------------------------------------------------------------------
              if (ReadCase())
              {
                if (ReadCsePerson1())
                {
                  // ***************************************************************
                  // A CP that is not an organization was found. Set the Flag to
                  // Y.
                  // ***************************************************************
                  local.CpCsePersonFound.Flag = "Y";
                }
              }
            }
            else if (AsChar(entities.CpCsePerson.Type1) == 'C')
            {
              // ********************************************************
              // The individual CP is found. Set the Flag to Y.
              // ********************************************************
              local.CpCsePersonFound.Flag = "Y";
            }

            if (AsChar(local.CpCsePersonFound.Flag) == 'Y')
            {
              local.CpParticipantRecord.RecordType = "5";
              local.CpParticipantRecord.Type1 = "I";
              local.CsePersonsWorkSet.Number = entities.CpCsePerson.Number;

              // ************************************************
              // *Call EAB to retrieve information about a CSE  *
              // *PERSON from the ADABAS system.                *
              // ************************************************
              UseCabReadAdabasPersonBatch2();

              if (Equal(local.AbendData.AdabasResponseCd, "0148"))
              {
                UseFnB691PrintErrorLine();
                ExitState = "ADABAS_UNAVAILABLE_RB";

                return;
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseFnB691PrintErrorLine();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.ErrorFound.Flag = "Y";
              }

              local.CpParticipantRecord.Role = "CP";
              local.CpParticipantRecord.FirstName =
                local.CsePersonsWorkSet.FirstName;
              local.CpParticipantRecord.MiddleInitial =
                local.CsePersonsWorkSet.MiddleInitial;
              local.CpParticipantRecord.LastName =
                local.CsePersonsWorkSet.LastName;
              local.CpParticipantRecord.Ssn = local.CsePersonsWorkSet.Ssn;
              local.CpParticipantRecord.Gender = local.CsePersonsWorkSet.Sex;
              local.CpParticipantRecord.SrsPersonNumber =
                entities.CpCsePerson.Number;
              local.CpParticipantRecord.FamilyViolenceIndicator = "";
              local.CpParticipantRecord.DateOfBirth =
                NumberToString(DateToInt(local.CsePersonsWorkSet.Dob), 8, 8);
              local.CpParticipantRecord.Pin = "00000000";
              local.CpParticipantRecord.Source = "SRS";
            }
            else if (AsChar(local.CpCsePersonFound.Flag) == 'N')
            {
              // ******************************************
              // If no CP is found, set the TPP to SRS
              // ******************************************
              local.CpThirdPartyRecord.RecordType = "5";
              local.CpThirdPartyRecord.Type1 = "A";
              local.CpThirdPartyRecord.Role = "TPP";
              local.CpThirdPartyRecord.AgencyName = "SRS";
              local.CpThirdPartyRecord.SrsPersonNumber = "";
              local.CpThirdPartyRecord.Pin = "";
              local.CpThirdPartyRecord.Source = "SRS";
            }
          }
          else
          {
            // ******************************************
            // If no CP is found, set the TPP to SRS
            // ******************************************
            local.CpThirdPartyRecord.RecordType = "5";
            local.CpThirdPartyRecord.Type1 = "A";
            local.CpThirdPartyRecord.Role = "TPP";
            local.CpThirdPartyRecord.AgencyName = "SRS";
            local.CpThirdPartyRecord.SrsPersonNumber = "";
            local.CpThirdPartyRecord.Pin = "";
            local.CpThirdPartyRecord.Source = "SRS";
          }
        }
      }

      if (Equal(local.Hold.Code, "SP") || Equal(local.Hold.Code, "SAJ"))
      {
        if (ReadCsePerson2())
        {
          local.CpParticipantRecord.RecordType = "5";
          local.CpParticipantRecord.Type1 = "I";
          local.CsePersonsWorkSet.Number = entities.CpCsePerson.Number;

          // ************************************************
          // *Call EAB to retrieve information about a CSE  *
          // *PERSON from the ADABAS system.                *
          // ************************************************
          UseCabReadAdabasPersonBatch2();

          if (Equal(local.AbendData.AdabasResponseCd, "0148"))
          {
            UseFnB691PrintErrorLine();
            ExitState = "ADABAS_UNAVAILABLE_RB";

            return;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseFnB691PrintErrorLine();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            local.ErrorFound.Flag = "Y";
          }

          local.CpParticipantRecord.Role = "CP";
          local.CpParticipantRecord.FirstName =
            local.CsePersonsWorkSet.FirstName;
          local.CpParticipantRecord.MiddleInitial =
            local.CsePersonsWorkSet.MiddleInitial;
          local.CpParticipantRecord.LastName = local.CsePersonsWorkSet.LastName;
          local.CpParticipantRecord.Ssn = local.CsePersonsWorkSet.Ssn;
          local.CpParticipantRecord.Gender = local.CsePersonsWorkSet.Sex;
          local.CpParticipantRecord.SrsPersonNumber =
            entities.CpCsePerson.Number;
          local.CpParticipantRecord.FamilyViolenceIndicator = "";
          local.CpParticipantRecord.DateOfBirth =
            NumberToString(DateToInt(local.CsePersonsWorkSet.Dob), 8, 8);
          local.CpParticipantRecord.Pin = "00000000";
          local.CpParticipantRecord.Source = "SRS";
        }
        else
        {
          // ******************************************
          // If no CP is found, set the TPP to SRS
          // ******************************************
          local.CpThirdPartyRecord.RecordType = "5";
          local.CpThirdPartyRecord.Type1 = "A";
          local.CpThirdPartyRecord.Role = "TPP";
          local.CpThirdPartyRecord.AgencyName = "SRS";
          local.CpThirdPartyRecord.SrsPersonNumber = "";
          local.CpThirdPartyRecord.Pin = "";
          local.CpThirdPartyRecord.Source = "SRS";
        }
      }

      // ******************************************************
      // Write CP Participant Record to Output File
      // ******************************************************
      local.PrintFileRecord.CourtOrderLine = "";

      if (!IsEmpty(local.CpParticipantRecord.RecordType))
      {
        local.PrintFileRecord.CourtOrderLine =
          local.CpParticipantRecord.RecordType + local
          .CpParticipantRecord.Role + local.CpParticipantRecord.Type1 + local
          .CpParticipantRecord.Ssn + local.CpParticipantRecord.LastName + local
          .CpParticipantRecord.FirstName + local
          .CpParticipantRecord.MiddleInitial + local
          .CpParticipantRecord.Suffix + local.CpParticipantRecord.Gender + local
          .CpParticipantRecord.DateOfBirth + local
          .CpParticipantRecord.SrsPersonNumber + local
          .CpParticipantRecord.FamilyViolenceIndicator + local
          .CpParticipantRecord.Pin + local.CpParticipantRecord.Source + local
          .CpParticipantRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }
      }

      // ********************
      // Get CP address
      // ********************
      if (entities.CpCsePerson.Populated)
      {
        if (AsChar(entities.CpCsePerson.Type1) == 'O')
        {
          goto Test;
        }

        if (ReadCsePersonAddress2())
        {
          local.CpAddressRecord.RecordType = "6";
          local.CpAddressRecord.Type1 = "M";
          local.CpAddressRecord.Source = "SRS";
          local.CpAddressRecord.City = entities.CsePersonAddress.City ?? Spaces
            (20);
          local.CpAddressRecord.Street = entities.CsePersonAddress.Street1 ?? Spaces
            (30);
          local.CpAddressRecord.Street2 = entities.CsePersonAddress.Street2 ?? Spaces
            (30);

          if (AsChar(entities.CsePersonAddress.LocationType) == 'D')
          {
            local.CpAddressRecord.Province = "";
            local.CpAddressRecord.Country = "";
            local.CpAddressRecord.State = entities.CsePersonAddress.State ?? Spaces
              (2);

            if (IsEmpty(entities.CsePersonAddress.Zip4))
            {
              local.CpAddressRecord.PostalCode =
                entities.CsePersonAddress.ZipCode ?? Spaces(10);
            }
            else
            {
              local.CpAddressRecord.PostalCode =
                entities.CsePersonAddress.ZipCode + "-" + entities
                .CsePersonAddress.Zip4;
            }
          }
          else if (AsChar(entities.CsePersonAddress.LocationType) == 'F')
          {
            local.CpAddressRecord.Province =
              entities.CsePersonAddress.Province ?? Spaces(5);
            local.CpAddressRecord.Country =
              entities.CsePersonAddress.Country ?? Spaces(20);
            local.CpAddressRecord.State = "";
            local.CpAddressRecord.PostalCode =
              entities.CsePersonAddress.PostalCode ?? Spaces(10);
          }
        }

        // ******************************************************
        // Write CP Address Records to Output File
        // ******************************************************
        if (!IsEmpty(local.CpAddressRecord.RecordType))
        {
          local.PrintFileRecord.CourtOrderLine = "";
          local.PrintFileRecord.CourtOrderLine =
            local.CpAddressRecord.RecordType + local.CpAddressRecord.Street + local
            .CpAddressRecord.Street2 + local.CpAddressRecord.City + local
            .CpAddressRecord.State + local.CpAddressRecord.PostalCode + local
            .CpAddressRecord.Country + local.CpAddressRecord.PhoneNumber + local
            .CpAddressRecord.Province + local.CpAddressRecord.Source + local
            .CpAddressRecord.Type1 + local.CpAddressRecord.Filler;
          UseFnExtWriteInterfaceFile();
          ++export.TotalWritten.Count;

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }
        }
      }

Test:

      if (Equal(local.CpThirdPartyRecord.AgencyName, "SRS"))
      {
        // ******************************************************
        // Write Third Party Participant Record to Output File
        // ******************************************************
        local.ThirdPartyRecord.RecordType = "5";
        local.ThirdPartyRecord.Role = "TPP";
        local.ThirdPartyRecord.Type1 = "A";
        local.ThirdPartyRecord.SrsPersonNumber = "";
        local.ThirdPartyRecord.AgencyName = "SRS";
        local.ThirdPartyRecord.Pin = "";
        local.ThirdPartyRecord.Source = "SRS";
        local.PrintFileRecord.CourtOrderLine = "";
        local.PrintFileRecord.CourtOrderLine =
          local.ThirdPartyRecord.RecordType + local.ThirdPartyRecord.Role + local
          .ThirdPartyRecord.Type1 + local.ThirdPartyRecord.SrsPersonNumber + local
          .ThirdPartyRecord.AgencyName + local.ThirdPartyRecord.Pin + local
          .ThirdPartyRecord.Source + local.ThirdPartyRecord.Filler;
        UseFnExtWriteInterfaceFile();
        ++export.TotalWritten.Count;

        if (!IsEmpty(local.Return1.Parm1))
        {
          ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

          return;
        }
      }

      // ***************************
      // Get the CHILD information.
      // ***************************
      if (Equal(local.Hold.Code, "SP") || Equal(local.Hold.Code, "SAJ"))
      {
      }
      else
      {
        foreach(var item1 in ReadCsePerson5())
        {
          local.ChldParticipantRecord.RecordType = "5";
          local.ChldParticipantRecord.Type1 = "I";
          local.CsePersonsWorkSet.Number = entities.ChildCsePerson.Number;

          // ************************************************
          // *Call EAB to retrieve information about a CSE  *
          // *PERSON from the ADABAS system.                *
          // ************************************************
          UseCabReadAdabasPersonBatch2();

          if (Equal(local.AbendData.AdabasResponseCd, "0148"))
          {
            UseFnB691PrintErrorLine();
            ExitState = "ADABAS_UNAVAILABLE_RB";

            return;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseFnB691PrintErrorLine();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            local.ErrorFound.Flag = "Y";
          }

          local.ChldParticipantRecord.Role = "CHLD";
          local.ChldParticipantRecord.FirstName =
            local.CsePersonsWorkSet.FirstName;
          local.ChldParticipantRecord.MiddleInitial =
            local.CsePersonsWorkSet.MiddleInitial;
          local.ChldParticipantRecord.LastName =
            local.CsePersonsWorkSet.LastName;
          local.ChldParticipantRecord.Ssn = local.CsePersonsWorkSet.Ssn;
          local.ChldParticipantRecord.Gender = local.CsePersonsWorkSet.Sex;
          local.ChldParticipantRecord.SrsPersonNumber =
            entities.ChildCsePerson.Number;
          local.ChldParticipantRecord.FamilyViolenceIndicator = "";
          local.ChldParticipantRecord.DateOfBirth =
            NumberToString(DateToInt(local.CsePersonsWorkSet.Dob), 8, 8);
          local.ChldParticipantRecord.Pin = "00000000";
          local.ChldParticipantRecord.Source = "SRS";

          // ******************************************************
          // Write CHILD Participant Record to Output File
          // ******************************************************
          local.PrintFileRecord.CourtOrderLine = "";
          local.PrintFileRecord.CourtOrderLine =
            local.ChldParticipantRecord.RecordType + local
            .ChldParticipantRecord.Role + local.ChldParticipantRecord.Type1 + local
            .ChldParticipantRecord.Ssn + local
            .ChldParticipantRecord.LastName + local
            .ChldParticipantRecord.FirstName + local
            .ChldParticipantRecord.MiddleInitial + local
            .ChldParticipantRecord.Suffix + local
            .ChldParticipantRecord.Gender + local
            .ChldParticipantRecord.DateOfBirth + local
            .ChldParticipantRecord.SrsPersonNumber + local
            .ChldParticipantRecord.FamilyViolenceIndicator + local
            .ChldParticipantRecord.Pin + local.ChldParticipantRecord.Source + local
            .ChldParticipantRecord.Filler;
          UseFnExtWriteInterfaceFile();
          ++export.TotalWritten.Count;

          if (!IsEmpty(local.Return1.Parm1))
          {
            ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

            return;
          }

          // ***************************
          // Get the CHILD address.
          // ***************************
          if (ReadCsePersonAddress1())
          {
            local.ChldAddressRecord.RecordType = "6";
            local.ChldAddressRecord.Type1 = "M";
            local.ChldAddressRecord.Source = "SRS";
            local.ChldAddressRecord.City = entities.CsePersonAddress.City ?? Spaces
              (20);
            local.ChldAddressRecord.Street =
              entities.CsePersonAddress.Street1 ?? Spaces(30);
            local.ChldAddressRecord.Street2 =
              entities.CsePersonAddress.Street2 ?? Spaces(30);

            if (AsChar(entities.CsePersonAddress.LocationType) == 'D')
            {
              local.ChldAddressRecord.Province = "";
              local.ChldAddressRecord.Country = "";
              local.ChldAddressRecord.State =
                entities.CsePersonAddress.State ?? Spaces(2);

              if (IsEmpty(entities.CsePersonAddress.Zip4))
              {
                local.ChldAddressRecord.PostalCode =
                  entities.CsePersonAddress.ZipCode ?? Spaces(10);
              }
              else
              {
                local.ChldAddressRecord.PostalCode =
                  entities.CsePersonAddress.ZipCode + "-" + entities
                  .CsePersonAddress.Zip4;
              }
            }
            else if (AsChar(entities.CsePersonAddress.LocationType) == 'F')
            {
              local.ChldAddressRecord.Province =
                entities.CsePersonAddress.Province ?? Spaces(5);
              local.ChldAddressRecord.Country =
                entities.CsePersonAddress.Country ?? Spaces(20);
              local.ChldAddressRecord.State = "";
              local.ChldAddressRecord.PostalCode =
                entities.CsePersonAddress.PostalCode ?? Spaces(10);
            }
          }

          // ******************************************************
          // Write CHILD Address Records to Output File
          // ******************************************************
          local.PrintFileRecord.CourtOrderLine = "";

          if (!IsEmpty(local.ChldAddressRecord.RecordType))
          {
            local.PrintFileRecord.CourtOrderLine =
              local.ChldAddressRecord.RecordType + local
              .ChldAddressRecord.Street + local.ChldAddressRecord.Street2 + local
              .ChldAddressRecord.City + local.ChldAddressRecord.State + local
              .ChldAddressRecord.PostalCode + local
              .ChldAddressRecord.Country + local
              .ChldAddressRecord.PhoneNumber + local
              .ChldAddressRecord.Province + local.ChldAddressRecord.Source + local
              .ChldAddressRecord.Type1 + local.ChldAddressRecord.Filler;
            UseFnExtWriteInterfaceFile();
            ++export.TotalWritten.Count;

            if (!IsEmpty(local.Return1.Parm1))
            {
              ExitState = "FN0000_PRINT_FILE_RECORD_ERROR";

              return;
            }
          }
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
    local.Ae.Flag = useExport.Ae.Flag;
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
    local.Ae.Flag = useExport.Ae.Flag;
  }

  private void UseFnB691PrintErrorLine()
  {
    var useImport = new FnB691PrintErrorLine.Import();
    var useExport = new FnB691PrintErrorLine.Export();

    useImport.LegalAction.Assign(import.LegalAction);
    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB691PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    useImport.FileCount.Count = local.FileNumber.Count;
    MoveKpcExternalParms(local.Send, useImport.KpcExternalParms);
    useImport.PrintFileRecord.CourtOrderLine =
      local.PrintFileRecord.CourtOrderLine;
    MoveKpcExternalParms(local.Return1, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.Return1);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.CpLegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CpCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", import.Max.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CpCsePerson.Number = db.GetString(reader, 0);
        entities.CpCsePerson.Type1 = db.GetString(reader, 1);
        entities.CpCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CpCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CpCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CpCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(import.LegalActionDetail.Populated);
    entities.CpCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", import.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", import.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.CpCsePerson.Number = db.GetString(reader, 0);
        entities.CpCsePerson.Type1 = db.GetString(reader, 1);
        entities.CpCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CpCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CpCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CpCsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CpCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CpCsePerson.Number = db.GetString(reader, 0);
        entities.CpCsePerson.Type1 = db.GetString(reader, 1);
        entities.CpCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CpCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CpCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CpCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson5()
  {
    System.Diagnostics.Debug.Assert(import.LegalActionDetail.Populated);
    entities.ChildCsePerson.Populated = false;

    return ReadEach("ReadCsePerson5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", import.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", import.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.ChildCsePerson.Number = db.GetString(reader, 0);
        entities.ChildCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChildCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ChildCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.ChildCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildCsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 15);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CpCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 15);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress3()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ncp.Number);
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 15);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonLegalActionPerson1()
  {
    entities.CpCsePerson.Populated = false;
    entities.CpLegalActionPerson.Populated = false;

    return Read("ReadCsePersonLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CpCsePerson.Number = db.GetString(reader, 0);
        entities.CpLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 0);
        entities.CpCsePerson.Type1 = db.GetString(reader, 1);
        entities.CpCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CpCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CpLegalActionPerson.Identifier = db.GetInt32(reader, 4);
        entities.CpLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CpLegalActionPerson.Role = db.GetString(reader, 6);
        entities.CpLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.CpLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 8);
        entities.CpLegalActionPerson.AccountType =
          db.GetNullableString(reader, 9);
        entities.CpLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CpCsePerson.Populated = true;
        entities.CpLegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CpCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(import.LegalActionDetail.Populated);
    entities.NcpObligor.Populated = false;
    entities.Ncp.Populated = false;

    return ReadEach("ReadCsePersonLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", import.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladRNumber", import.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", import.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Ncp.Number = db.GetString(reader, 0);
        entities.NcpObligor.CspNumber = db.GetNullableString(reader, 0);
        entities.Ncp.Type1 = db.GetString(reader, 1);
        entities.Ncp.OrganizationName = db.GetNullableString(reader, 2);
        entities.Ncp.FamilyViolenceIndicator = db.GetNullableString(reader, 3);
        entities.NcpObligor.Identifier = db.GetInt32(reader, 4);
        entities.NcpObligor.LgaRIdentifier = db.GetNullableInt32(reader, 5);
        entities.NcpObligor.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.NcpObligor.AccountType = db.GetNullableString(reader, 7);
        entities.NcpObligor.Populated = true;
        entities.Ncp.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ncp.Type1);

        return true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.NcpRole.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", entities.Ncp.Number);
      },
      (db, reader) =>
      {
        entities.NcpRole.Identifier = db.GetInt32(reader, 0);
        entities.NcpRole.CspNumber = db.GetNullableString(reader, 1);
        entities.NcpRole.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.NcpRole.Role = db.GetString(reader, 3);
        entities.NcpRole.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(import.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of TotalWritten.
    /// </summary>
    [JsonPropertyName("totalWritten")]
    public Common TotalWritten
    {
      get => totalWritten ??= new();
      set => totalWritten = value;
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

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private DateWorkArea max;
    private Common totalWritten;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotalWritten.
    /// </summary>
    [JsonPropertyName("totalWritten")]
    public Common TotalWritten
    {
      get => totalWritten ??= new();
      set => totalWritten = value;
    }

    private Common totalWritten;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public KpcExternalParms Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of PrintFileRecord.
    /// </summary>
    [JsonPropertyName("printFileRecord")]
    public PrintFileRecord PrintFileRecord
    {
      get => printFileRecord ??= new();
      set => printFileRecord = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public KpcExternalParms Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of HoldNcp.
    /// </summary>
    [JsonPropertyName("holdNcp")]
    public CsePerson HoldNcp
    {
      get => holdNcp ??= new();
      set => holdNcp = value;
    }

    /// <summary>
    /// A value of NcpDebtRecord.
    /// </summary>
    [JsonPropertyName("ncpDebtRecord")]
    public NcpDebtRecord NcpDebtRecord
    {
      get => ncpDebtRecord ??= new();
      set => ncpDebtRecord = value;
    }

    /// <summary>
    /// A value of ObligorCount.
    /// </summary>
    [JsonPropertyName("obligorCount")]
    public Common ObligorCount
    {
      get => obligorCount ??= new();
      set => obligorCount = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public ObligationType Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of ObligationRecord.
    /// </summary>
    [JsonPropertyName("obligationRecord")]
    public ObligationRecord ObligationRecord
    {
      get => obligationRecord ??= new();
      set => obligationRecord = value;
    }

    /// <summary>
    /// A value of NcpParticipantRecord.
    /// </summary>
    [JsonPropertyName("ncpParticipantRecord")]
    public ParticipantRecord NcpParticipantRecord
    {
      get => ncpParticipantRecord ??= new();
      set => ncpParticipantRecord = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of NcpAddressRecord.
    /// </summary>
    [JsonPropertyName("ncpAddressRecord")]
    public AddressRecord NcpAddressRecord
    {
      get => ncpAddressRecord ??= new();
      set => ncpAddressRecord = value;
    }

    /// <summary>
    /// A value of CpThirdPartyRecord.
    /// </summary>
    [JsonPropertyName("cpThirdPartyRecord")]
    public ThirdPartyRecord CpThirdPartyRecord
    {
      get => cpThirdPartyRecord ??= new();
      set => cpThirdPartyRecord = value;
    }

    /// <summary>
    /// A value of CpParticipantRecord.
    /// </summary>
    [JsonPropertyName("cpParticipantRecord")]
    public ParticipantRecord CpParticipantRecord
    {
      get => cpParticipantRecord ??= new();
      set => cpParticipantRecord = value;
    }

    /// <summary>
    /// A value of CpCsePersonFound.
    /// </summary>
    [JsonPropertyName("cpCsePersonFound")]
    public Common CpCsePersonFound
    {
      get => cpCsePersonFound ??= new();
      set => cpCsePersonFound = value;
    }

    /// <summary>
    /// A value of CpAddressRecord.
    /// </summary>
    [JsonPropertyName("cpAddressRecord")]
    public AddressRecord CpAddressRecord
    {
      get => cpAddressRecord ??= new();
      set => cpAddressRecord = value;
    }

    /// <summary>
    /// A value of ThirdPartyRecord.
    /// </summary>
    [JsonPropertyName("thirdPartyRecord")]
    public ThirdPartyRecord ThirdPartyRecord
    {
      get => thirdPartyRecord ??= new();
      set => thirdPartyRecord = value;
    }

    /// <summary>
    /// A value of ChldParticipantRecord.
    /// </summary>
    [JsonPropertyName("chldParticipantRecord")]
    public ParticipantRecord ChldParticipantRecord
    {
      get => chldParticipantRecord ??= new();
      set => chldParticipantRecord = value;
    }

    /// <summary>
    /// A value of ChldAddressRecord.
    /// </summary>
    [JsonPropertyName("chldAddressRecord")]
    public AddressRecord ChldAddressRecord
    {
      get => chldAddressRecord ??= new();
      set => chldAddressRecord = value;
    }

    private Common fileNumber;
    private KpcExternalParms send;
    private PrintFileRecord printFileRecord;
    private KpcExternalParms return1;
    private CsePerson holdNcp;
    private NcpDebtRecord ncpDebtRecord;
    private Common obligorCount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common closeInd;
    private Common errorFound;
    private ObligationType hold;
    private ObligationRecord obligationRecord;
    private ParticipantRecord ncpParticipantRecord;
    private Common ae;
    private Common errOnAdabasUnavailable;
    private AddressRecord ncpAddressRecord;
    private ThirdPartyRecord cpThirdPartyRecord;
    private ParticipantRecord cpParticipantRecord;
    private Common cpCsePersonFound;
    private AddressRecord cpAddressRecord;
    private ThirdPartyRecord thirdPartyRecord;
    private ParticipantRecord chldParticipantRecord;
    private AddressRecord chldAddressRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of NcpRole.
    /// </summary>
    [JsonPropertyName("ncpRole")]
    public LegalActionPerson NcpRole
    {
      get => ncpRole ??= new();
      set => ncpRole = value;
    }

    /// <summary>
    /// A value of NcpObligor.
    /// </summary>
    [JsonPropertyName("ncpObligor")]
    public LegalActionPerson NcpObligor
    {
      get => ncpObligor ??= new();
      set => ncpObligor = value;
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

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePerson Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
    }

    /// <summary>
    /// A value of CpCsePerson.
    /// </summary>
    [JsonPropertyName("cpCsePerson")]
    public CsePerson CpCsePerson
    {
      get => cpCsePerson ??= new();
      set => cpCsePerson = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of CpLegalActionPerson.
    /// </summary>
    [JsonPropertyName("cpLegalActionPerson")]
    public LegalActionPerson CpLegalActionPerson
    {
      get => cpLegalActionPerson ??= new();
      set => cpLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ChildLegalActionPerson.
    /// </summary>
    [JsonPropertyName("childLegalActionPerson")]
    public LegalActionPerson ChildLegalActionPerson
    {
      get => childLegalActionPerson ??= new();
      set => childLegalActionPerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson ncpRole;
    private LegalActionPerson ncpObligor;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private CsePerson ncp;
    private CsePerson cpCsePerson;
    private CsePerson childCsePerson;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalActionPerson cpLegalActionPerson;
    private LegalActionPerson childLegalActionPerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
