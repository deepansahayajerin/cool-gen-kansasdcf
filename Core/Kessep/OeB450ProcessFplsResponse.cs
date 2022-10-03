// Program: OE_B450_PROCESS_FPLS_RESPONSE, ID: 371289202, model: 746.
// Short name: SWE01973
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B450_PROCESS_FPLS_RESPONSE.
/// </summary>
[Serializable]
public partial class OeB450ProcessFplsResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B450_PROCESS_FPLS_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB450ProcessFplsResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB450ProcessFplsResponse.
  /// </summary>
  public OeB450ProcessFplsResponse(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    //                             M A I N T E N A N C E    L O G
    // ---------------------------------------------------------------------------------------------
    //  Date    Developer	Request #  Description
    // -------- ----------	---------- 
    // ----------------------------------------------------------
    // ??/??/??  ?????? 	????????  Initial Development
    // 06/05/09  DDupree	CQ7189	  Added check when processing the returning ssn 
    // to see if it
    // 				  is an invalid ssn and person number combination. Part of CQ7189.
    // 02/15/10  GVandy	CQ6659	  NSA employment is now considered "verified 
    // employment".
    // 				  Set return code to "E"mployed and the return date to processing 
    // date.
    // 				  Auto IWOs will now be generated for NSA employment.
    // 04/28/10  LSS	        CQ7536    Checked for an existing Response to get 
    // the Infrastructure detail
    //                                   
    // "FPLS Locate Response Received
    // Date" date from and if there is no
    //                                   
    // existing response use the process
    // date.
    //                                   
    // Get the Agency Code from the new
    // Response not an existing Response.
    // ---------------------------------------------------------------------------------------------
    export.FplsRequestsCreated.Count = import.FplsRequestsCreated.Count;
    export.FplsRequestsUpdated.Count = import.FplsRequestsUpdated.Count;
    export.FplsResponsesCreated.Count = import.FplsResponsesCreated.Count;
    export.FplsAlertsCreated.Count = import.FplsAlertsCreated.Count;
    export.FplsResponsesSkipped.Count = import.FplsResponsesSkipped.Count;
    export.DatabaseActivity.Count = import.DatabaseActivity.Count;
    local.Current.Date = Now().Date;
    local.FplsLocateResponse.SubmittingOffice =
      import.ExternalFplsResponse.SubmittingOffice;
    local.FplsLocateResponse.DodAnnualSalary =
      import.ExternalFplsResponse.DodAnnualSalary;
    local.FplsLocateResponse.DodEligibilityCode =
      import.ExternalFplsResponse.DodEligibilityCode;
    local.Blank.Date = new DateTime(1, 1, 1);
    local.Blank.Timestamp = new DateTime(1, 1, 1);

    // ************************************************
    // *If date of address format ind                 *
    // *  (all dates now converted in EAB)            *
    // *= 0 then no date is available.                *
    // *= 1 then CCYYMM00 format                      *
    // *= 2 then CCYYQR00 format                      *
    // *= 3 then CCYY0000 format                      *
    // *= 4 then CCYYMMDD format                      *
    // ************************************************
    switch(AsChar(import.ExternalFplsResponse.AddrDateFormatInd))
    {
      case '1':
        local.FplsLocateResponse.DateOfAddress =
          import.ExternalFplsResponse.DateOfAddress;

        break;
      case '2':
        local.FplsLocateResponse.DateOfAddress =
          import.ExternalFplsResponse.DateOfAddress;

        break;
      case '3':
        local.FplsLocateResponse.DateOfAddress =
          import.ExternalFplsResponse.DateOfAddress;

        break;
      case '4':
        local.FplsLocateResponse.DateOfAddress =
          import.ExternalFplsResponse.DateOfAddress;

        break;
      default:
        local.FplsLocateResponse.DateOfAddress = local.NullDate.Date;

        break;
    }

    // ************************************************
    // *       DOD Date of Death                      *
    // ************************************************
    local.FplsLocateResponse.DodDateOfDeathOrSeparation =
      import.ExternalFplsResponse.DodDateOfDeathOrSeparation;

    // ************************************************
    // *        VA Date of Death                      *
    // ************************************************
    local.FplsLocateResponse.VaDateOfDeath =
      import.ExternalFplsResponse.VaDateOfDeath;

    // ************************************************
    // *        VA Effective date of award.           *
    // ************************************************
    local.FplsLocateResponse.VaAmtOfAwardEffectiveDate =
      import.ExternalFplsResponse.VaAmtOfAwardEffectiveDate;

    // ************************************************
    // *        MBR Date of Death.                    *
    // ************************************************
    local.FplsLocateResponse.MbrDateOfDeath =
      import.ExternalFplsResponse.MbrDateOfDeath;

    // ************************************************
    // *        DOD Date of Birth                     *
    // ************************************************
    local.FplsLocateResponse.DodDateOfBirth =
      import.ExternalFplsResponse.DodDateOfBirth;

    // ************************************************
    // *         Date of Hire                         *
    // ************************************************
    local.FplsLocateResponse.DateOfHire =
      import.ExternalFplsResponse.DateOfHire;

    // ************************************************
    // *Move all information from the external to the *
    // *entity format for the FPLS response.          *
    // ************************************************
    local.FplsLocateResponse.AddrDateFormatInd =
      import.ExternalFplsResponse.AddrDateFormatInd;
    local.FplsLocateResponse.AddressFormatInd =
      import.ExternalFplsResponse.AddressFormatInd;
    local.FplsLocateResponse.AgencyCode =
      import.ExternalFplsResponse.AgencyCode;
    local.FplsLocateResponse.ApNameReturned =
      import.ExternalFplsResponse.ApNameReturned;
    local.FplsLocateResponse.DodPayGradeCode =
      import.ExternalFplsResponse.DodPayGradeCode;
    local.FplsLocateResponse.DodServiceCode =
      import.ExternalFplsResponse.DodServiceCode;
    local.FplsLocateResponse.DodStatus = import.ExternalFplsResponse.DodStatus;
    local.FplsLocateResponse.IrsNameControl =
      import.ExternalFplsResponse.IrsNameControl;
    local.FplsLocateResponse.NameSentInd =
      import.ExternalFplsResponse.NameSentInd;
    local.FplsLocateResponse.NprcEmpdOrSepd =
      import.ExternalFplsResponse.NprcEmpdOrSepd;
    local.FplsLocateResponse.ResponseCode =
      import.ExternalFplsResponse.ResponseCode;
    local.FplsLocateResponse.ReturnedAddress =
      import.ExternalFplsResponse.ReturnedAddress;
    local.FplsLocateResponse.SesaRespondingState =
      import.ExternalFplsResponse.SesaRespondingState;
    local.FplsLocateResponse.SesaWageClaimInd =
      import.ExternalFplsResponse.SesaWageClaimInd;
    local.FplsLocateResponse.SesaWageAmount =
      import.ExternalFplsResponse.SesaWageAmount;
    local.FplsLocateResponse.IrsTaxYear =
      (int?)StringToNumber(import.ExternalFplsResponse.IrsTaxYear);
    local.FplsLocateResponse.MbrBenefitAmount =
      import.ExternalFplsResponse.MbrBenefitAmount;
    local.FplsLocateResponse.VaAmountOfAward =
      import.ExternalFplsResponse.VaAmountOfAward;
    local.FplsLocateResponse.SsaCorpDivision =
      import.ExternalFplsResponse.SsaCorpDivision;
    local.FplsLocateResponse.SsaFederalOrMilitary =
      import.ExternalFplsResponse.SsaFederalOrMilitary;
    local.FplsLocateResponse.SsnReturned =
      import.ExternalFplsResponse.SsnSubmitted;
    local.FplsLocateResponse.VaBenefitCode =
      import.ExternalFplsResponse.VaBenefitCode;
    local.FplsLocateResponse.VaIncarcerationCode =
      import.ExternalFplsResponse.VaIncarcerationCode;
    local.FplsLocateResponse.VaRetirementPayCode =
      import.ExternalFplsResponse.VaRetirementPayCode;
    local.FplsLocateResponse.VaSuspenseCode =
      import.ExternalFplsResponse.VaSuspenseCode;
    local.FplsLocateResponse.NdnhResponse =
      import.ExternalFplsResponse.NdnhResponse;
    local.FplsLocateResponse.CorrectedAdditionMultipleSsn =
      import.ExternalFplsResponse.CorrectedAdditionMultipleSsn;
    local.FplsLocateResponse.AddressIndType =
      import.ExternalFplsResponse.AddressIndType;
    local.FplsLocateResponse.HealthInsBenefitIndicator =
      import.ExternalFplsResponse.HealthInsBenefitIndicator;
    local.FplsLocateResponse.EmploymentInd =
      import.ExternalFplsResponse.EmploymentInd;
    local.FplsLocateResponse.EmploymentStatus =
      import.ExternalFplsResponse.EmploymentStatus;
    local.FplsLocateResponse.ReportingFedAgency =
      import.ExternalFplsResponse.ReportingFedAgency;
    local.FplsLocateResponse.Fein = import.ExternalFplsResponse.Fein;
    local.FplsLocateResponse.SsnMatchInd =
      import.ExternalFplsResponse.SsnMatchInd;
    local.FplsLocateResponse.ReportingQuarter =
      import.ExternalFplsResponse.ReportingQuarter;
    local.NdnhProactiveMatch.Flag = "N";

    if (ReadCsePerson())
    {
      local.Convert.SsnNum9 =
        (int)StringToNumber(import.ExternalFplsResponse.SsnSubmitted);

      // added this check as part of cq7189.
      if (ReadInvalidSsn())
      {
        if (AsChar(import.ExternalFplsResponse.AddressIndType) == '2')
        {
          if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C' || AsChar
            (import.ExternalFplsResponse.AddressFormatInd) == 'X')
          {
            local.EmployerAddress.City =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 161, 15);
            local.EmployerAddress.State =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 191, 2);
            local.EmployerAddress.ZipCode =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 193, 5);
            local.EmployerAddress.Zip4 =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 4);

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'X')
            {
              local.EmployerAddress.Street1 =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 1, 40);

              if (!IsEmpty(Substring(
                import.ExternalFplsResponse.ReturnedAddress, 41, 40)))
              {
                local.EmployerAddress.Street2 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 41, 40);
                  
              }

              if (!IsEmpty(Substring(
                import.ExternalFplsResponse.ReturnedAddress, 81, 40)))
              {
                local.EmployerAddress.Street3 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 81, 40);
                  
              }

              if (!IsEmpty(Substring(
                import.ExternalFplsResponse.ReturnedAddress, 121, 40)))
              {
                local.EmployerAddress.Street4 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 81, 40);
                  
              }

              if (!IsEmpty(local.EmployerAddress.Street1))
              {
                do
                {
                  // This is to get rid of any leading spaces
                  if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                  {
                    local.EmployerAddress.Street1 =
                      Substring(local.EmployerAddress.Street1, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street1 =
                      TrimEnd(local.EmployerAddress.Street1);

                    goto Test1;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test1:

              if (!IsEmpty(local.EmployerAddress.Street2))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                  {
                    local.EmployerAddress.Street2 =
                      Substring(local.EmployerAddress.Street2, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street2 =
                      TrimEnd(local.EmployerAddress.Street2);

                    goto Test2;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test2:

              if (!IsEmpty(local.EmployerAddress.Street3))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                  {
                    local.EmployerAddress.Street3 =
                      Substring(local.EmployerAddress.Street3, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street3 =
                      TrimEnd(local.EmployerAddress.Street3);

                    goto Test3;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test3:

              if (!IsEmpty(local.EmployerAddress.Street4))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.EmployerAddress.Street4, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street4 =
                      TrimEnd(local.EmployerAddress.Street4);

                    goto Test4;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }
            }

Test4:

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C')
            {
              local.ReturnAddress.ReturnedAddress =
                import.ExternalFplsResponse.ReturnedAddress;

              do
              {
                local.EndPointer.Count =
                  Find(local.ReturnAddress.ReturnedAddress, "\\");

                if (local.EndPointer.Count == 0)
                {
                  if (!IsEmpty(local.ReturnAddress.ReturnedAddress))
                  {
                    if (local.TotalCount.Count == 1)
                    {
                      local.EmployerAddress.Street2 =
                        TrimEnd(local.ReturnAddress.ReturnedAddress);
                    }
                    else if (local.TotalCount.Count == 2)
                    {
                      local.EmployerAddress.Street3 =
                        TrimEnd(local.ReturnAddress.ReturnedAddress);
                    }
                    else if (local.TotalCount.Count == 3)
                    {
                      local.EmployerAddress.Street4 =
                        TrimEnd(local.ReturnAddress.ReturnedAddress);
                    }
                    else
                    {
                      // if there is anything else there is no place to put it 
                      // so we will have to drop it
                    }
                  }

                  break;
                }
                else
                {
                  ++local.TotalCount.Count;
                }

                if (local.TotalCount.Count == 1)
                {
                  local.EmployerAddress.Street1 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                if (local.TotalCount.Count == 2)
                {
                  local.EmployerAddress.Street2 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                if (local.TotalCount.Count == 3)
                {
                  local.EmployerAddress.Street3 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                if (local.TotalCount.Count == 4)
                {
                  local.EmployerAddress.Street4 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);

                  break;
                }

                local.ReturnAddress.ReturnedAddress =
                  Substring(local.ReturnAddress.ReturnedAddress,
                  local.EndPointer.Count +
                  1, FplsLocateResponse.ReturnedAddress_MaxLength -
                  local.EndPointer.Count);
              }
              while(!Equal(global.Command, "COMMAND"));

              if (local.TotalCount.Count == 0 && IsEmpty
                (local.EmployerAddress.Street1) && !
                IsEmpty(local.ReturnAddress.ReturnedAddress))
              {
                local.EmployerAddress.Street1 =
                  TrimEnd(local.ReturnAddress.ReturnedAddress);
              }
              else
              {
                if (!IsEmpty(local.EmployerAddress.Street1))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                    {
                      local.EmployerAddress.Street1 =
                        Substring(local.EmployerAddress.Street1, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street1 =
                        TrimEnd(local.EmployerAddress.Street1);

                      goto Test5;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test5:

                if (!IsEmpty(local.EmployerAddress.Street2))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                    {
                      local.EmployerAddress.Street2 =
                        Substring(local.EmployerAddress.Street2, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street2 =
                        TrimEnd(local.EmployerAddress.Street2);

                      goto Test6;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test6:

                if (!IsEmpty(local.EmployerAddress.Street3))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                    {
                      local.EmployerAddress.Street3 =
                        Substring(local.EmployerAddress.Street3, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street3 =
                        TrimEnd(local.EmployerAddress.Street3);

                      goto Test7;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test7:

                if (!IsEmpty(local.EmployerAddress.Street4))
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.EmployerAddress.Street4, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street4 =
                      TrimEnd(local.EmployerAddress.Street4);
                  }
                }
              }
            }
          }

          if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'F')
          {
            local.ReturnAddress.ReturnedAddress =
              import.ExternalFplsResponse.ReturnedAddress;

            do
            {
              local.EndPointer.Count =
                Find(local.ReturnAddress.ReturnedAddress, "\\");

              if (local.EndPointer.Count == 0)
              {
                if (!IsEmpty(Substring(
                  local.ReturnAddress.ReturnedAddress, 1, 40)))
                {
                  // ***********************************************************************************************
                  // This is done so we will catch  the last part of an address 
                  // that does not have a end pointer after it.
                  // ***********************************************************************************************
                  ++local.TotalCount.Count;

                  goto Test8;
                }

                break;
              }
              else
              {
                ++local.TotalCount.Count;
              }

Test8:

              if (local.TotalCount.Count == 1)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street1 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street1 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 2)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street2 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street2 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 3)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street3 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street3 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 4)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street4 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street4 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 5)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.City =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.City =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 6)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.State =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.State =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                break;
              }

              local.ReturnAddress.ReturnedAddress =
                Substring(local.ReturnAddress.ReturnedAddress,
                local.EndPointer.Count +
                1, FplsLocateResponse.ReturnedAddress_MaxLength -
                local.EndPointer.Count);
            }
            while(!Equal(global.Command, "COMMAND"));

            switch(local.TotalCount.Count)
            {
              case 0:
                if (local.TotalCount.Count == 0 && IsEmpty
                  (local.EmployerAddress.Street1) && !
                  IsEmpty(local.ReturnAddress.ReturnedAddress))
                {
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);
                }

                break;
              case 1:
                // leave as is, it is not a complete  address, we can possible 
                // get the city and state from the zip code
                break;
              case 2:
                // leave as is, it is not a complete  address, we can possible 
                // get the city and state from the zip code
                break;
              case 3:
                local.EmployerAddress.State = local.EmployerAddress.Street3 ?? ""
                  ;
                local.EmployerAddress.City = local.EmployerAddress.Street2 ?? ""
                  ;
                local.EmployerAddress.Street3 = "";
                local.EmployerAddress.Street2 = "";
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              case 4:
                local.EmployerAddress.State = local.EmployerAddress.Street4 ?? ""
                  ;
                local.EmployerAddress.City = local.EmployerAddress.Street3 ?? ""
                  ;
                local.EmployerAddress.Street4 = "";
                local.EmployerAddress.Street3 = "";
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street1);
                local.EmployerAddress.Street2 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              case 5:
                local.EmployerAddress.State = local.EmployerAddress.City ?? "";
                local.EmployerAddress.City = local.EmployerAddress.Street4 ?? ""
                  ;
                local.EmployerAddress.Street4 = "";
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street1);
                local.EmployerAddress.Street2 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.Street3 =
                  TrimEnd(local.EmployerAddress.Street3);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              case 6:
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street1);
                local.EmployerAddress.Street2 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.Street3 =
                  TrimEnd(local.EmployerAddress.Street3);
                local.EmployerAddress.Street4 =
                  TrimEnd(local.EmployerAddress.Street4);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              default:
                // this is an error
                break;
            }

            if (!IsEmpty(local.EmployerAddress.Street1))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                {
                  local.EmployerAddress.Street1 =
                    Substring(local.EmployerAddress.Street1, 2, 24);
                }
                else
                {
                  goto Test9;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test9:

            if (!IsEmpty(local.EmployerAddress.Street2))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                {
                  local.EmployerAddress.Street2 =
                    Substring(local.EmployerAddress.Street2, 2, 24);
                }
                else
                {
                  goto Test10;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test10:

            if (!IsEmpty(local.EmployerAddress.Street3))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                {
                  local.EmployerAddress.Street3 =
                    Substring(local.EmployerAddress.Street3, 2, 24);
                }
                else
                {
                  goto Test11;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test11:

            if (!IsEmpty(local.EmployerAddress.Street4))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                {
                  local.EmployerAddress.Street4 =
                    Substring(local.EmployerAddress.Street4, 2, 24);
                }
                else
                {
                  goto Test12;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test12:

            if (!IsEmpty(local.EmployerAddress.City))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.City, 1, 1)))
                {
                  local.EmployerAddress.City =
                    Substring(local.EmployerAddress.City, 2, 14);
                }
                else
                {
                  goto Test13;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test13:

            if (!IsEmpty(local.EmployerAddress.State))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.State, 1, 1)))
                {
                  local.EmployerAddress.State =
                    Substring(local.EmployerAddress.State, 2, 1);
                }
                else
                {
                  goto Test14;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test14:

            local.EmployerAddress.ZipCode =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 193, 5);
            local.EmployerAddress.Zip4 =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 4);
          }
        }
        else if (AsChar(import.ExternalFplsResponse.AddressIndType) == '1')
        {
          local.Employment.StartDt = import.ExternalFplsResponse.DateOfHire;
          local.Employment.EndDt =
            import.ExternalFplsResponse.DodDateOfDeathOrSeparation;

          if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C' || AsChar
            (import.ExternalFplsResponse.AddressFormatInd) == 'X')
          {
            local.EmployerAddress.City =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 166, 15);
            local.EmployerAddress.State =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 196, 2);
            local.EmployerAddress.ZipCode =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 5);
            local.EmployerAddress.Zip4 =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 203, 4);

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'X')
            {
              local.EmployerAddress.Street1 =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 46, 40);

              if (!IsEmpty(Substring(
                import.ExternalFplsResponse.ReturnedAddress, 86, 40)))
              {
                local.EmployerAddress.Street2 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 86, 40);
                  
              }

              if (!IsEmpty(Substring(
                import.ExternalFplsResponse.ReturnedAddress, 126, 40)))
              {
                local.EmployerAddress.Street3 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 126, 40);
                  
              }

              if (!IsEmpty(local.EmployerAddress.Street1))
              {
                do
                {
                  // This is to get rid of any leading spaces
                  if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                  {
                    local.EmployerAddress.Street1 =
                      Substring(local.EmployerAddress.Street1, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street1 =
                      TrimEnd(local.EmployerAddress.Street1);

                    goto Test15;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test15:

              if (!IsEmpty(local.EmployerAddress.Street2))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                  {
                    local.EmployerAddress.Street2 =
                      Substring(local.EmployerAddress.Street2, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street2 =
                      TrimEnd(local.EmployerAddress.Street2);

                    goto Test16;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test16:

              if (!IsEmpty(local.EmployerAddress.Street3))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                  {
                    local.EmployerAddress.Street3 =
                      Substring(local.EmployerAddress.Street3, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street3 =
                      TrimEnd(local.EmployerAddress.Street3);

                    goto Test17;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }
            }

Test17:

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C')
            {
              local.ReturnAddress.ReturnedAddress =
                import.ExternalFplsResponse.ReturnedAddress;

              // ***********************************************************************************************
              // Now we have to account for the employer's name that has been 
              // placed in the
              // first 45 characters. this was done because there was no 
              // employer's name field in
              // the fpls locate table, so it is being passed in throught the 
              // return address field.
              // This means that is a street address 4 came in the fille, it is 
              // now lost.
              // ***********************************************************************************************
              local.ReturnAddress.ReturnedAddress =
                Substring(local.ReturnAddress.ReturnedAddress, 46, 198);

              do
              {
                local.EndPointer.Count =
                  Find(local.ReturnAddress.ReturnedAddress, "\\");

                if (local.EndPointer.Count == 0)
                {
                  if (!IsEmpty(local.ReturnAddress.ReturnedAddress))
                  {
                    if (local.TotalCount.Count == 1)
                    {
                      local.EmployerAddress.Street2 =
                        TrimEnd(local.ReturnAddress.ReturnedAddress);
                    }
                    else if (local.TotalCount.Count == 2)
                    {
                      local.EmployerAddress.Street3 =
                        TrimEnd(local.ReturnAddress.ReturnedAddress);
                    }
                    else if (local.TotalCount.Count == 3)
                    {
                      local.EmployerAddress.Street4 =
                        TrimEnd(local.ReturnAddress.ReturnedAddress);
                    }
                    else
                    {
                      // if there is anything else there is no place to put it 
                      // so we will have to drop it
                    }
                  }

                  break;
                }
                else
                {
                  ++local.TotalCount.Count;
                }

                if (local.TotalCount.Count == 1)
                {
                  local.EmployerAddress.Street1 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                if (local.TotalCount.Count == 2)
                {
                  local.EmployerAddress.Street2 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                if (local.TotalCount.Count == 3)
                {
                  local.EmployerAddress.Street3 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                if (local.TotalCount.Count == 4)
                {
                  local.EmployerAddress.Street4 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);

                  break;
                }

                local.ReturnAddress.ReturnedAddress =
                  Substring(local.ReturnAddress.ReturnedAddress,
                  local.EndPointer.Count +
                  1, FplsLocateResponse.ReturnedAddress_MaxLength -
                  local.EndPointer.Count);
              }
              while(!Equal(global.Command, "COMMAND"));

              if (local.TotalCount.Count == 0 && IsEmpty
                (local.EmployerAddress.Street1) && !
                IsEmpty(local.ReturnAddress.ReturnedAddress))
              {
                local.EmployerAddress.Street1 =
                  TrimEnd(local.ReturnAddress.ReturnedAddress);
              }
              else
              {
                if (!IsEmpty(local.EmployerAddress.Street1))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                    {
                      local.EmployerAddress.Street1 =
                        Substring(local.EmployerAddress.Street1, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street1 =
                        TrimEnd(local.EmployerAddress.Street1);

                      goto Test18;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test18:

                if (!IsEmpty(local.EmployerAddress.Street2))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                    {
                      local.EmployerAddress.Street2 =
                        Substring(local.EmployerAddress.Street2, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street2 =
                        TrimEnd(local.EmployerAddress.Street2);

                      goto Test19;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test19:

                if (!IsEmpty(local.EmployerAddress.Street3))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                    {
                      local.EmployerAddress.Street3 =
                        Substring(local.EmployerAddress.Street3, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street3 =
                        TrimEnd(local.EmployerAddress.Street3);

                      goto Test20;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test20:

                if (!IsEmpty(local.EmployerAddress.Street4))
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.EmployerAddress.Street4, 2, 24);
                  }
                  else
                  {
                    local.EmployerAddress.Street4 =
                      TrimEnd(local.EmployerAddress.Street4);
                  }
                }
              }
            }
          }

          if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'F')
          {
            local.ReturnAddress.ReturnedAddress =
              import.ExternalFplsResponse.ReturnedAddress;

            // ***********************************************************************************************
            // Now we have to account for the employer's name that has been 
            // placed in the
            // first 45 characters. this was done because there was no employer'
            // s name field in
            // the fpls locate table, so it is being passed in throught the 
            // return address field.
            // This means that is a street address 4 came in the file, it is now
            // lost.
            // ***********************************************************************************************
            local.ReturnAddress.ReturnedAddress =
              Substring(local.ReturnAddress.ReturnedAddress, 46, 198);

            do
            {
              local.EndPointer.Count =
                Find(local.ReturnAddress.ReturnedAddress, "\\");

              if (local.EndPointer.Count == 0)
              {
                if (!IsEmpty(Substring(
                  local.ReturnAddress.ReturnedAddress, 1, 40)))
                {
                  // ***********************************************************************************************
                  // This is done so we will catch  the last part of an address 
                  // that does not have a end pointer after it.
                  // ***********************************************************************************************
                  ++local.TotalCount.Count;

                  goto Test21;
                }

                break;
              }
              else
              {
                ++local.TotalCount.Count;
              }

Test21:

              if (local.TotalCount.Count == 1)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street1 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street1 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 2)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street2 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street2 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 3)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street3 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street3 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 4)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.Street4 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.Street4 =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 5)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.City =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.City =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }
              }

              if (local.TotalCount.Count == 6)
              {
                if (local.EndPointer.Count == 0)
                {
                  if (IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    --local.TotalCount.Count;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

                  local.EmployerAddress.State =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);

                  break;
                }
                else
                {
                  do
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 1)))
                    {
                      local.ReturnAddress.ReturnedAddress =
                        Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                      --local.EndPointer.Count;
                    }
                    else
                    {
                      break;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  local.EmployerAddress.State =
                    Substring(local.ReturnAddress.ReturnedAddress, 1,
                    local.EndPointer.Count - 1);
                }

                break;
              }

              local.ReturnAddress.ReturnedAddress =
                Substring(local.ReturnAddress.ReturnedAddress,
                local.EndPointer.Count +
                1, FplsLocateResponse.ReturnedAddress_MaxLength -
                local.EndPointer.Count);
            }
            while(!Equal(global.Command, "COMMAND"));

            switch(local.TotalCount.Count)
            {
              case 0:
                if (local.TotalCount.Count == 0 && IsEmpty
                  (local.EmployerAddress.Street1) && !
                  IsEmpty(local.ReturnAddress.ReturnedAddress))
                {
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);
                }

                break;
              case 1:
                // leave as is, it is not a complete  address, we can possible 
                // get the city and state from the zip code
                break;
              case 2:
                // leave as is, it is not a complete  address, we can possible 
                // get the city and state from the zip code
                break;
              case 3:
                local.EmployerAddress.State = local.EmployerAddress.Street3 ?? ""
                  ;
                local.EmployerAddress.City = local.EmployerAddress.Street2 ?? ""
                  ;
                local.EmployerAddress.Street3 = "";
                local.EmployerAddress.Street2 = "";
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              case 4:
                local.EmployerAddress.State = local.EmployerAddress.Street4 ?? ""
                  ;
                local.EmployerAddress.City = local.EmployerAddress.Street3 ?? ""
                  ;
                local.EmployerAddress.Street4 = "";
                local.EmployerAddress.Street3 = "";
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street1);
                local.EmployerAddress.Street2 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              case 5:
                local.EmployerAddress.State = local.EmployerAddress.City ?? "";
                local.EmployerAddress.City = local.EmployerAddress.Street4 ?? ""
                  ;
                local.EmployerAddress.Street4 = "";
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street1);
                local.EmployerAddress.Street2 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.Street3 =
                  TrimEnd(local.EmployerAddress.Street3);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              case 6:
                local.EmployerAddress.Street1 =
                  TrimEnd(local.EmployerAddress.Street1);
                local.EmployerAddress.Street2 =
                  TrimEnd(local.EmployerAddress.Street2);
                local.EmployerAddress.Street3 =
                  TrimEnd(local.EmployerAddress.Street3);
                local.EmployerAddress.Street4 =
                  TrimEnd(local.EmployerAddress.Street4);
                local.EmployerAddress.State =
                  TrimEnd(local.EmployerAddress.State);
                local.EmployerAddress.City =
                  TrimEnd(local.EmployerAddress.City);

                break;
              default:
                // this is an error
                break;
            }

            if (!IsEmpty(local.EmployerAddress.Street1))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                {
                  local.EmployerAddress.Street1 =
                    Substring(local.EmployerAddress.Street1, 2, 24);
                }
                else
                {
                  goto Test22;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test22:

            if (!IsEmpty(local.EmployerAddress.Street2))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                {
                  local.EmployerAddress.Street2 =
                    Substring(local.EmployerAddress.Street2, 2, 24);
                }
                else
                {
                  goto Test23;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test23:

            if (!IsEmpty(local.EmployerAddress.Street3))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                {
                  local.EmployerAddress.Street3 =
                    Substring(local.EmployerAddress.Street3, 2, 24);
                }
                else
                {
                  goto Test24;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test24:

            if (!IsEmpty(local.EmployerAddress.Street4))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                {
                  local.EmployerAddress.Street4 =
                    Substring(local.EmployerAddress.Street4, 2, 24);
                }
                else
                {
                  goto Test25;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test25:

            if (!IsEmpty(local.EmployerAddress.City))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.City, 1, 1)))
                {
                  local.EmployerAddress.City =
                    Substring(local.EmployerAddress.City, 2, 14);
                }
                else
                {
                  goto Test26;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test26:

            if (!IsEmpty(local.EmployerAddress.State))
            {
              do
              {
                if (IsEmpty(Substring(local.EmployerAddress.State, 1, 1)))
                {
                  local.EmployerAddress.State =
                    Substring(local.EmployerAddress.State, 2, 1);
                }
                else
                {
                  goto Test27;
                }
              }
              while(!Equal(global.Command, "COMMAND"));
            }

Test27:

            local.EmployerAddress.ZipCode =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 5);
            local.EmployerAddress.Zip4 =
              Substring(import.ExternalFplsResponse.ReturnedAddress, 203, 4);
          }
        }

        local.Message1.Text18 = "Rec FPLS -person #";
        local.Message1.Text14 = " with bad ssn";
        local.Message1.Text2 = ":";
        local.Message1.Text3 = " -";
        local.Message2.Text2 = ",";
        local.Message2.Text1 = "";

        switch(TrimEnd(import.ExternalFplsResponse.AgencyCode))
        {
          case "A01":
            local.Message2.Text3 = "DOD";

            break;
          case "A02":
            local.Message2.Text3 = "FBI";

            break;
          case "A03":
            local.Message2.Text3 = "NSA";

            break;
          case "C01":
            local.Message2.Text3 = "IRS";

            break;
          case "E01":
            local.Message2.Text3 = "SSA";

            break;
          case "F01":
            local.Message2.Text3 = "VA";

            break;
          default:
            local.Message2.Text3 = "";

            break;
        }

        local.EabReportSend.RptDetail = local.Message1.Text18 + import
          .ExternalFplsResponse.ApCsePersonNumber + local.Message1.Text14 + import
          .ExternalFplsResponse.SsnSubmitted + local.Message1.Text2 + local
          .Message2.Text3 + local.Message1.Text3;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local.Message2.Text1 + TrimEnd
          (local.EmployerAddress.Street1) + local.Message2.Text2 + TrimEnd
          (local.EmployerAddress.City) + local.Message2.Text2 + (
            local.EmployerAddress.State ?? "") + local.Message2.Text1 + (
            local.EmployerAddress.ZipCode ?? "");
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }

        return;
      }
      else
      {
        // this is fine, there is not invalid ssn record for this combination of
        // cse person number and ssn number
      }

      if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
      {
        // ***********************************************************************************************
        // 11/08/2005               DDupree               WR00258947
        // All the code inside the "IF" statement to see if the agency code is =
        // A03 is new.
        // It is for the fcr minor 05-02 release.
        // ***********************************************************************************************
        local.CsePerson.Assign(entities.CsePerson);
        local.FcrPersonAckErrorRecord.MemberId = entities.CsePerson.Number;
        local.FplsSourceName.Text30 =
          Substring(import.ExternalFplsResponse.ReturnedAddress, 1, 45);
        local.Max.Date = new DateTime(2099, 12, 31);
        local.EmployerAddress.LocationType = "D";

        if (Lt(local.Blank.Date, import.ExternalFplsResponse.NsaDateOfDeath))
        {
          if (AsChar(import.ExternalFplsResponse.AddressIndType) == '2')
          {
            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C' || AsChar
              (import.ExternalFplsResponse.AddressFormatInd) == 'X')
            {
              local.EmployerAddress.City =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 161, 15);
                
              local.EmployerAddress.State =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 191, 2);
              local.EmployerAddress.ZipCode =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 193, 5);
              local.EmployerAddress.Zip4 =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 4);

              if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'X')
              {
                local.EmployerAddress.Street1 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 1, 40);
                  

                if (!IsEmpty(Substring(
                  import.ExternalFplsResponse.ReturnedAddress, 41, 40)))
                {
                  local.EmployerAddress.Street2 =
                    Substring(import.ExternalFplsResponse.ReturnedAddress, 41,
                    40);
                }

                if (!IsEmpty(Substring(
                  import.ExternalFplsResponse.ReturnedAddress, 81, 40)))
                {
                  local.EmployerAddress.Street3 =
                    Substring(import.ExternalFplsResponse.ReturnedAddress, 81,
                    40);
                }

                if (!IsEmpty(Substring(
                  import.ExternalFplsResponse.ReturnedAddress, 121, 40)))
                {
                  local.EmployerAddress.Street4 =
                    Substring(import.ExternalFplsResponse.ReturnedAddress, 81,
                    40);
                }

                if (!IsEmpty(local.EmployerAddress.Street1))
                {
                  do
                  {
                    // This is to get rid of any leading spaces
                    if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                    {
                      local.EmployerAddress.Street1 =
                        Substring(local.EmployerAddress.Street1, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street1 =
                        TrimEnd(local.EmployerAddress.Street1);

                      goto Test28;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test28:

                if (!IsEmpty(local.EmployerAddress.Street2))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                    {
                      local.EmployerAddress.Street2 =
                        Substring(local.EmployerAddress.Street2, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street2 =
                        TrimEnd(local.EmployerAddress.Street2);

                      goto Test29;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test29:

                if (!IsEmpty(local.EmployerAddress.Street3))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                    {
                      local.EmployerAddress.Street3 =
                        Substring(local.EmployerAddress.Street3, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street3 =
                        TrimEnd(local.EmployerAddress.Street3);

                      goto Test30;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test30:

                if (!IsEmpty(local.EmployerAddress.Street4))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                    {
                      local.EmployerAddress.Street4 =
                        Substring(local.EmployerAddress.Street4, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street4 =
                        TrimEnd(local.EmployerAddress.Street4);

                      goto Test31;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }
              }

Test31:

              if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C')
              {
                local.ReturnAddress.ReturnedAddress =
                  import.ExternalFplsResponse.ReturnedAddress;

                do
                {
                  local.EndPointer.Count =
                    Find(local.ReturnAddress.ReturnedAddress, "\\");

                  if (local.EndPointer.Count == 0)
                  {
                    if (!IsEmpty(local.ReturnAddress.ReturnedAddress))
                    {
                      if (local.TotalCount.Count == 1)
                      {
                        local.EmployerAddress.Street2 =
                          TrimEnd(local.ReturnAddress.ReturnedAddress);
                      }
                      else if (local.TotalCount.Count == 2)
                      {
                        local.EmployerAddress.Street3 =
                          TrimEnd(local.ReturnAddress.ReturnedAddress);
                      }
                      else if (local.TotalCount.Count == 3)
                      {
                        local.EmployerAddress.Street4 =
                          TrimEnd(local.ReturnAddress.ReturnedAddress);
                      }
                      else
                      {
                        // if there is anything else there is no place to put it
                        // so we will have to drop it
                      }
                    }

                    break;
                  }
                  else
                  {
                    ++local.TotalCount.Count;
                  }

                  if (local.TotalCount.Count == 1)
                  {
                    local.EmployerAddress.Street1 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  if (local.TotalCount.Count == 2)
                  {
                    local.EmployerAddress.Street2 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  if (local.TotalCount.Count == 3)
                  {
                    local.EmployerAddress.Street3 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  if (local.TotalCount.Count == 4)
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);

                    break;
                  }

                  local.ReturnAddress.ReturnedAddress =
                    Substring(local.ReturnAddress.ReturnedAddress,
                    local.EndPointer.Count +
                    1, FplsLocateResponse.ReturnedAddress_MaxLength -
                    local.EndPointer.Count);
                }
                while(!Equal(global.Command, "COMMAND"));

                if (local.TotalCount.Count == 0 && IsEmpty
                  (local.EmployerAddress.Street1) && !
                  IsEmpty(local.ReturnAddress.ReturnedAddress))
                {
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);
                }
                else
                {
                  if (!IsEmpty(local.EmployerAddress.Street1))
                  {
                    do
                    {
                      if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                        
                      {
                        local.EmployerAddress.Street1 =
                          Substring(local.EmployerAddress.Street1, 2, 24);
                      }
                      else
                      {
                        local.EmployerAddress.Street1 =
                          TrimEnd(local.EmployerAddress.Street1);

                        goto Test32;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

Test32:

                  if (!IsEmpty(local.EmployerAddress.Street2))
                  {
                    do
                    {
                      if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                        
                      {
                        local.EmployerAddress.Street2 =
                          Substring(local.EmployerAddress.Street2, 2, 24);
                      }
                      else
                      {
                        local.EmployerAddress.Street2 =
                          TrimEnd(local.EmployerAddress.Street2);

                        goto Test33;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

Test33:

                  if (!IsEmpty(local.EmployerAddress.Street3))
                  {
                    do
                    {
                      if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                        
                      {
                        local.EmployerAddress.Street3 =
                          Substring(local.EmployerAddress.Street3, 2, 24);
                      }
                      else
                      {
                        local.EmployerAddress.Street3 =
                          TrimEnd(local.EmployerAddress.Street3);

                        goto Test34;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

Test34:

                  if (!IsEmpty(local.EmployerAddress.Street4))
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                    {
                      local.EmployerAddress.Street4 =
                        Substring(local.EmployerAddress.Street4, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street4 =
                        TrimEnd(local.EmployerAddress.Street4);
                    }
                  }
                }
              }
            }

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'F')
            {
              local.ReturnAddress.ReturnedAddress =
                import.ExternalFplsResponse.ReturnedAddress;

              do
              {
                local.EndPointer.Count =
                  Find(local.ReturnAddress.ReturnedAddress, "\\");

                if (local.EndPointer.Count == 0)
                {
                  if (!IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    // ***********************************************************************************************
                    // This is done so we will catch  the last part of an 
                    // address that does not have a end pointer after it.
                    // ***********************************************************************************************
                    ++local.TotalCount.Count;

                    goto Test35;
                  }

                  break;
                }
                else
                {
                  ++local.TotalCount.Count;
                }

Test35:

                if (local.TotalCount.Count == 1)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street1 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street1 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 2)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street2 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street2 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 3)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street3 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street3 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 4)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street4 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street4 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 5)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.City =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.City =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 6)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.State =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.State =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  break;
                }

                local.ReturnAddress.ReturnedAddress =
                  Substring(local.ReturnAddress.ReturnedAddress,
                  local.EndPointer.Count +
                  1, FplsLocateResponse.ReturnedAddress_MaxLength -
                  local.EndPointer.Count);
              }
              while(!Equal(global.Command, "COMMAND"));

              switch(local.TotalCount.Count)
              {
                case 0:
                  if (local.TotalCount.Count == 0 && IsEmpty
                    (local.EmployerAddress.Street1) && !
                    IsEmpty(local.ReturnAddress.ReturnedAddress))
                  {
                    local.EmployerAddress.Street1 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);
                  }

                  break;
                case 1:
                  // leave as is, it is not a complete  address, we can possible
                  // get the city and state from the zip code
                  break;
                case 2:
                  // leave as is, it is not a complete  address, we can possible
                  // get the city and state from the zip code
                  break;
                case 3:
                  local.EmployerAddress.State =
                    local.EmployerAddress.Street3 ?? "";
                  local.EmployerAddress.City =
                    local.EmployerAddress.Street2 ?? "";
                  local.EmployerAddress.Street3 = "";
                  local.EmployerAddress.Street2 = "";
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                case 4:
                  local.EmployerAddress.State =
                    local.EmployerAddress.Street4 ?? "";
                  local.EmployerAddress.City =
                    local.EmployerAddress.Street3 ?? "";
                  local.EmployerAddress.Street4 = "";
                  local.EmployerAddress.Street3 = "";
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street1);
                  local.EmployerAddress.Street2 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                case 5:
                  local.EmployerAddress.State = local.EmployerAddress.City ?? ""
                    ;
                  local.EmployerAddress.City =
                    local.EmployerAddress.Street4 ?? "";
                  local.EmployerAddress.Street4 = "";
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street1);
                  local.EmployerAddress.Street2 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.Street3 =
                    TrimEnd(local.EmployerAddress.Street3);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                case 6:
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street1);
                  local.EmployerAddress.Street2 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.Street3 =
                    TrimEnd(local.EmployerAddress.Street3);
                  local.EmployerAddress.Street4 =
                    TrimEnd(local.EmployerAddress.Street4);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                default:
                  // this is an error
                  break;
              }

              if (!IsEmpty(local.EmployerAddress.Street1))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                  {
                    local.EmployerAddress.Street1 =
                      Substring(local.EmployerAddress.Street1, 2, 24);
                  }
                  else
                  {
                    goto Test36;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test36:

              if (!IsEmpty(local.EmployerAddress.Street2))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                  {
                    local.EmployerAddress.Street2 =
                      Substring(local.EmployerAddress.Street2, 2, 24);
                  }
                  else
                  {
                    goto Test37;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test37:

              if (!IsEmpty(local.EmployerAddress.Street3))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                  {
                    local.EmployerAddress.Street3 =
                      Substring(local.EmployerAddress.Street3, 2, 24);
                  }
                  else
                  {
                    goto Test38;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test38:

              if (!IsEmpty(local.EmployerAddress.Street4))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.EmployerAddress.Street4, 2, 24);
                  }
                  else
                  {
                    goto Test39;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test39:

              if (!IsEmpty(local.EmployerAddress.City))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.City, 1, 1)))
                  {
                    local.EmployerAddress.City =
                      Substring(local.EmployerAddress.City, 2, 14);
                  }
                  else
                  {
                    goto Test40;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test40:

              if (!IsEmpty(local.EmployerAddress.State))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.State, 1, 1)))
                  {
                    local.EmployerAddress.State =
                      Substring(local.EmployerAddress.State, 2, 1);
                  }
                  else
                  {
                    goto Test41;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test41:

              local.EmployerAddress.ZipCode =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 193, 5);
              local.EmployerAddress.Zip4 =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 4);
            }
          }

          local.FcrPersonAckErrorRecord.DateOfDeath =
            import.ExternalFplsResponse.NsaDateOfDeath;
          local.FcrPersonAckErrorRecord.FirstName =
            import.ExternalFplsResponse.ApFirstName;
          local.FcrPersonAckErrorRecord.MiddleName =
            import.ExternalFplsResponse.ApMiddleName;
          local.FcrPersonAckErrorRecord.LastName =
            import.ExternalFplsResponse.ApFirstLastName;

          if (!IsEmpty(import.ExternalFplsResponse.ApNameReturned))
          {
            local.FcrPersonAckErrorRecord.FcrPrimaryLastName =
              import.ExternalFplsResponse.ApFirstLastName;
          }

          local.FcrPersonAckErrorRecord.SsaZipCodeOfLastResidence =
            local.EmployerAddress.ZipCode ?? Spaces(5);
          local.NsaCityLastResidence.Text15 = local.EmployerAddress.City ?? Spaces
            (15);
          local.NsaStateLastResidence.Text2 = local.EmployerAddress.State ?? Spaces
            (2);

          switch(AsChar(import.DateOfDeathIndicator.Text1))
          {
            case 'Y':
              // ***********************************************************************************************
              // Date of Death has been reported by National Security Agency
              // This process is a mirror of how it is processed in the fcr 
              // incoming programming (b412).
              // ***********************************************************************************************
              local.ConvertDateDateWorkArea.Date =
                import.ExternalFplsResponse.NsaDateOfDeath;
              UseCabConvertDate2String();

              foreach(var item in ReadCaseRoleCase())
              {
                local.FcrPersonAckErrorRecord.CaseId = entities.Case1.Number;
                UseCabProcessDateOfDeath();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              break;
            case 'R':
              break;
            case 'I':
              // ***********************************************************************************************
              // Date of Death reported by National Security Agency is invalid
              // (date of death is less than or equal to date of birth)
              // ***********************************************************************************************
              local.ConvertDateDateWorkArea.Date =
                import.ExternalFplsResponse.NsaDateOfDeath;
              UseCabConvertDate2String();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Invalid date of death for person: " + entities
                .CsePerson.Number + " date reported as: " + local
                .ConvertDateTextWorkArea.Text8;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              break;
            default:
              break;
          }
        }
        else
        {
          // ***********************************************************************************************
          // This employment processing is based on how the employment 
          // processing is
          // handled in the New Hire and Quartely report programs (B273 and B274
          // ).
          // ***********************************************************************************************
          // WE ARE NOW DEALING WITH EMPLOYMENT
          local.CurrentMonth.Count =
            Month(import.ProgramProcessingInfo.ProcessDate);
          local.DetermineQtr.Year =
            Year(import.ProgramProcessingInfo.ProcessDate);
          local.DetermineQtr.Date = import.ProgramProcessingInfo.ProcessDate;
          local.YearHired.Year = Year(import.ExternalFplsResponse.DateOfHire);
          local.HiredMonth.Count =
            Month(import.ExternalFplsResponse.DateOfHire);
          local.Quater.Text1 = NumberToString(local.CurrentMonth.Count, 15, 1);

          if (Lt(AddYears(import.ProgramProcessingInfo.ProcessDate, -1),
            import.ExternalFplsResponse.DateOfHire))
          {
            local.LessThanAYear.Flag = "Y";
          }

          if (import.ExternalFplsResponse.DodAnnualSalary > 0)
          {
            local.DetermineSalary.TotalCurrency =
              (decimal)import.ExternalFplsResponse.DodAnnualSalary / 4;
          }
          else
          {
            local.DetermineSalary.TotalCurrency = 0;
          }

          local.Wages.AverageCurrency = local.DetermineSalary.TotalCurrency;

          if (AsChar(local.LessThanAYear.Flag) == 'Y')
          {
            // THIS IS FOR LESS THAN A FULL YEAR'S EMPLOYMENT
            if (local.CurrentMonth.Count >= 1 && local.CurrentMonth.Count <= 3)
            {
              if (local.HiredMonth.Count >= 1 && local.HiredMonth.Count <= 3)
              {
                if (local.YearHired.Year == local.DetermineQtr.Year)
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                }
                else
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.Attribute2NdQtr = "2";
                  local.Employment.Attribute3RdQtr = "3";
                  local.Employment.Attribute4ThQtr = "4";
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute2NdQtrYr =
                    local.DetermineQtr.Year - 1;
                  local.Employment.Attribute3RdQtrYr =
                    local.DetermineQtr.Year - 1;
                  local.Employment.Attribute4ThQtrYr =
                    local.DetermineQtr.Year - 1;
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute2NdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute3RdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute4ThQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                }
              }
              else if (local.HiredMonth.Count >= 4 && local
                .HiredMonth.Count <= 6)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.Attribute4ThQtr = "4";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute4ThQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 7 && local
                .HiredMonth.Count <= 9)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 10 && local
                .HiredMonth.Count <= 12)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
            }
            else if (local.CurrentMonth.Count >= 4 && local
              .CurrentMonth.Count <= 6)
            {
              if (local.HiredMonth.Count >= 1 && local.HiredMonth.Count <= 3)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 4 && local
                .HiredMonth.Count <= 6)
              {
                if (local.YearHired.Year == local.DetermineQtr.Year)
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                }
                else
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.Attribute2NdQtr = "2";
                  local.Employment.Attribute3RdQtr = "3";
                  local.Employment.Attribute4ThQtr = "4";
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute2NdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute3RdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute4ThQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute3RdQtrYr =
                    local.DetermineQtr.Year - 1;
                  local.Employment.Attribute4ThQtrYr =
                    local.DetermineQtr.Year - 1;
                }
              }
              else if (local.HiredMonth.Count >= 7 && local
                .HiredMonth.Count <= 9)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.Attribute4ThQtr = "4";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute4ThQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 10 && local
                .HiredMonth.Count <= 12)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
            }
            else if (local.CurrentMonth.Count >= 7 && local
              .CurrentMonth.Count <= 9)
            {
              if (local.HiredMonth.Count >= 1 && local.HiredMonth.Count <= 3)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 4 && local
                .HiredMonth.Count <= 6)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
              }
              else if (local.HiredMonth.Count >= 7 && local
                .HiredMonth.Count <= 9)
              {
                if (local.YearHired.Year == local.DetermineQtr.Year)
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                }
                else
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.Attribute2NdQtr = "2";
                  local.Employment.Attribute3RdQtr = "3";
                  local.Employment.Attribute4ThQtr = "4";
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute2NdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute3RdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute4ThQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute4ThQtrYr =
                    local.DetermineQtr.Year - 1;
                }
              }
              else if (local.HiredMonth.Count >= 10 && local
                .HiredMonth.Count <= 12)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.Attribute4ThQtr = "4";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year - 1
                  ;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute4ThQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
            }
            else if (local.CurrentMonth.Count >= 10 && local
              .CurrentMonth.Count <= 12)
            {
              if (local.HiredMonth.Count >= 1 && local.HiredMonth.Count <= 3)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.Attribute4ThQtr = "4";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute4ThQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 4 && local
                .HiredMonth.Count <= 6)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.Attribute3RdQtr = "3";
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute3RdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
              }
              else if (local.HiredMonth.Count >= 7 && local
                .HiredMonth.Count <= 9)
              {
                local.Employment.LastQtr = "1";
                local.Employment.Attribute2NdQtr = "2";
                local.Employment.LastQtrYr = local.DetermineQtr.Year;
                local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                local.Employment.LastQtrIncome =
                  local.DetermineSalary.TotalCurrency;
                local.Employment.Attribute2NdQtrIncome =
                  local.DetermineSalary.TotalCurrency;
              }
              else if (local.HiredMonth.Count >= 10 && local
                .HiredMonth.Count <= 12)
              {
                if (local.YearHired.Year == local.DetermineQtr.Year)
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                }
                else
                {
                  local.Employment.LastQtr = "1";
                  local.Employment.Attribute2NdQtr = "2";
                  local.Employment.Attribute3RdQtr = "3";
                  local.Employment.Attribute4ThQtr = "4";
                  local.Employment.LastQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
                  local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year;
                  local.Employment.LastQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute2NdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute3RdQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                  local.Employment.Attribute4ThQtrIncome =
                    local.DetermineSalary.TotalCurrency;
                }
              }
            }
          }
          else
          {
            // THIS IS FOR A FULL YEAR'S EMPLOYMENT
            local.Employment.LastQtr = "1";
            local.Employment.Attribute2NdQtr = "2";
            local.Employment.Attribute3RdQtr = "3";
            local.Employment.Attribute4ThQtr = "4";
            local.Employment.LastQtrIncome =
              local.DetermineSalary.TotalCurrency;
            local.Employment.Attribute2NdQtrIncome =
              local.DetermineSalary.TotalCurrency;
            local.Employment.Attribute3RdQtrIncome =
              local.DetermineSalary.TotalCurrency;
            local.Employment.Attribute4ThQtrIncome =
              local.DetermineSalary.TotalCurrency;

            if (local.HiredMonth.Count >= 1 && local.HiredMonth.Count <= 3)
            {
              local.Employment.LastQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year;
            }
            else if (local.HiredMonth.Count >= 4 && local.HiredMonth.Count <= 6)
            {
              local.Employment.LastQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year - 1;
            }
            else if (local.HiredMonth.Count >= 7 && local.HiredMonth.Count <= 9)
            {
              local.Employment.LastQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year - 1;
              local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year - 1;
            }
            else if (local.HiredMonth.Count >= 10 && local.HiredMonth.Count <= 12
              )
            {
              local.Employment.LastQtrYr = local.DetermineQtr.Year;
              local.Employment.Attribute2NdQtrYr = local.DetermineQtr.Year - 1;
              local.Employment.Attribute3RdQtrYr = local.DetermineQtr.Year - 1;
              local.Employment.Attribute4ThQtrYr = local.DetermineQtr.Year - 1;
            }
          }

          local.RecordUpdated.Flag = "";
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          local.AutomaticGenerateIwo.Flag = "N";
          local.AddressSuitableForIwo.Flag = "Y";
          local.DateOfHire.Date = import.ExternalFplsResponse.DateOfHire;
          local.Employer.Name =
            Substring(import.ExternalFplsResponse.ReturnedAddress, 1, 45);

          if (AsChar(import.ExternalFplsResponse.AddressIndType) == '1')
          {
            local.Employment.StartDt = import.ExternalFplsResponse.DateOfHire;
            local.Employment.EndDt =
              import.ExternalFplsResponse.DodDateOfDeathOrSeparation;

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C' || AsChar
              (import.ExternalFplsResponse.AddressFormatInd) == 'X')
            {
              local.EmployerAddress.City =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 166, 15);
                
              local.EmployerAddress.State =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 196, 2);
              local.EmployerAddress.ZipCode =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 5);
              local.EmployerAddress.Zip4 =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 203, 4);

              if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'X')
              {
                local.EmployerAddress.Street1 =
                  Substring(import.ExternalFplsResponse.ReturnedAddress, 46, 40);
                  

                if (!IsEmpty(Substring(
                  import.ExternalFplsResponse.ReturnedAddress, 86, 40)))
                {
                  local.EmployerAddress.Street2 =
                    Substring(import.ExternalFplsResponse.ReturnedAddress, 86,
                    40);
                }

                if (!IsEmpty(Substring(
                  import.ExternalFplsResponse.ReturnedAddress, 126, 40)))
                {
                  local.EmployerAddress.Street3 =
                    Substring(import.ExternalFplsResponse.ReturnedAddress, 126,
                    40);
                }

                if (!IsEmpty(local.EmployerAddress.Street1))
                {
                  do
                  {
                    // This is to get rid of any leading spaces
                    if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                    {
                      local.EmployerAddress.Street1 =
                        Substring(local.EmployerAddress.Street1, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street1 =
                        TrimEnd(local.EmployerAddress.Street1);

                      goto Test42;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test42:

                if (!IsEmpty(local.EmployerAddress.Street2))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                    {
                      local.EmployerAddress.Street2 =
                        Substring(local.EmployerAddress.Street2, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street2 =
                        TrimEnd(local.EmployerAddress.Street2);

                      goto Test43;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }

Test43:

                if (!IsEmpty(local.EmployerAddress.Street3))
                {
                  do
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                    {
                      local.EmployerAddress.Street3 =
                        Substring(local.EmployerAddress.Street3, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street3 =
                        TrimEnd(local.EmployerAddress.Street3);

                      goto Test44;
                    }
                  }
                  while(!Equal(global.Command, "COMMAND"));
                }
              }

Test44:

              if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'C')
              {
                local.ReturnAddress.ReturnedAddress =
                  import.ExternalFplsResponse.ReturnedAddress;

                // ***********************************************************************************************
                // Now we have to account for the employer's name that has been 
                // placed in the
                // first 45 characters. this was done because there was no 
                // employer's name field in
                // the fpls locate table, so it is being passed in throught the 
                // return address field.
                // This means that is a street address 4 came in the fille, it 
                // is now lost.
                // ***********************************************************************************************
                local.ReturnAddress.ReturnedAddress =
                  Substring(local.ReturnAddress.ReturnedAddress, 46, 198);

                do
                {
                  local.EndPointer.Count =
                    Find(local.ReturnAddress.ReturnedAddress, "\\");

                  if (local.EndPointer.Count == 0)
                  {
                    if (!IsEmpty(local.ReturnAddress.ReturnedAddress))
                    {
                      if (local.TotalCount.Count == 1)
                      {
                        local.EmployerAddress.Street2 =
                          TrimEnd(local.ReturnAddress.ReturnedAddress);
                      }
                      else if (local.TotalCount.Count == 2)
                      {
                        local.EmployerAddress.Street3 =
                          TrimEnd(local.ReturnAddress.ReturnedAddress);
                      }
                      else if (local.TotalCount.Count == 3)
                      {
                        local.EmployerAddress.Street4 =
                          TrimEnd(local.ReturnAddress.ReturnedAddress);
                      }
                      else
                      {
                        // if there is anything else there is no place to put it
                        // so we will have to drop it
                      }
                    }

                    break;
                  }
                  else
                  {
                    ++local.TotalCount.Count;
                  }

                  if (local.TotalCount.Count == 1)
                  {
                    local.EmployerAddress.Street1 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  if (local.TotalCount.Count == 2)
                  {
                    local.EmployerAddress.Street2 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  if (local.TotalCount.Count == 3)
                  {
                    local.EmployerAddress.Street3 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  if (local.TotalCount.Count == 4)
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);

                    break;
                  }

                  local.ReturnAddress.ReturnedAddress =
                    Substring(local.ReturnAddress.ReturnedAddress,
                    local.EndPointer.Count +
                    1, FplsLocateResponse.ReturnedAddress_MaxLength -
                    local.EndPointer.Count);
                }
                while(!Equal(global.Command, "COMMAND"));

                if (local.TotalCount.Count == 0 && IsEmpty
                  (local.EmployerAddress.Street1) && !
                  IsEmpty(local.ReturnAddress.ReturnedAddress))
                {
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.ReturnAddress.ReturnedAddress);
                }
                else
                {
                  if (!IsEmpty(local.EmployerAddress.Street1))
                  {
                    do
                    {
                      if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                        
                      {
                        local.EmployerAddress.Street1 =
                          Substring(local.EmployerAddress.Street1, 2, 24);
                      }
                      else
                      {
                        local.EmployerAddress.Street1 =
                          TrimEnd(local.EmployerAddress.Street1);

                        goto Test45;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

Test45:

                  if (!IsEmpty(local.EmployerAddress.Street2))
                  {
                    do
                    {
                      if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                        
                      {
                        local.EmployerAddress.Street2 =
                          Substring(local.EmployerAddress.Street2, 2, 24);
                      }
                      else
                      {
                        local.EmployerAddress.Street2 =
                          TrimEnd(local.EmployerAddress.Street2);

                        goto Test46;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

Test46:

                  if (!IsEmpty(local.EmployerAddress.Street3))
                  {
                    do
                    {
                      if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                        
                      {
                        local.EmployerAddress.Street3 =
                          Substring(local.EmployerAddress.Street3, 2, 24);
                      }
                      else
                      {
                        local.EmployerAddress.Street3 =
                          TrimEnd(local.EmployerAddress.Street3);

                        goto Test47;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));
                  }

Test47:

                  if (!IsEmpty(local.EmployerAddress.Street4))
                  {
                    if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                    {
                      local.EmployerAddress.Street4 =
                        Substring(local.EmployerAddress.Street4, 2, 24);
                    }
                    else
                    {
                      local.EmployerAddress.Street4 =
                        TrimEnd(local.EmployerAddress.Street4);
                    }
                  }
                }
              }
            }

            if (AsChar(import.ExternalFplsResponse.AddressFormatInd) == 'F')
            {
              local.ReturnAddress.ReturnedAddress =
                import.ExternalFplsResponse.ReturnedAddress;

              // ***********************************************************************************************
              // Now we have to account for the employer's name that has been 
              // placed in the
              // first 45 characters. this was done because there was no 
              // employer's name field in
              // the fpls locate table, so it is being passed in throught the 
              // return address field.
              // This means that is a street address 4 came in the file, it is 
              // now lost.
              // ***********************************************************************************************
              local.ReturnAddress.ReturnedAddress =
                Substring(local.ReturnAddress.ReturnedAddress, 46, 198);

              do
              {
                local.EndPointer.Count =
                  Find(local.ReturnAddress.ReturnedAddress, "\\");

                if (local.EndPointer.Count == 0)
                {
                  if (!IsEmpty(Substring(
                    local.ReturnAddress.ReturnedAddress, 1, 40)))
                  {
                    // ***********************************************************************************************
                    // This is done so we will catch  the last part of an 
                    // address that does not have a end pointer after it.
                    // ***********************************************************************************************
                    ++local.TotalCount.Count;

                    goto Test48;
                  }

                  break;
                }
                else
                {
                  ++local.TotalCount.Count;
                }

Test48:

                if (local.TotalCount.Count == 1)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street1 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street1 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 2)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street2 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street2 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 3)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street3 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street3 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 4)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.Street4 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.Street4 =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 5)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.City =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.City =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }
                }

                if (local.TotalCount.Count == 6)
                {
                  if (local.EndPointer.Count == 0)
                  {
                    if (IsEmpty(Substring(
                      local.ReturnAddress.ReturnedAddress, 1, 40)))
                    {
                      --local.TotalCount.Count;
                    }
                    else
                    {
                      do
                      {
                        if (IsEmpty(Substring(
                          local.ReturnAddress.ReturnedAddress, 1, 1)))
                        {
                          local.ReturnAddress.ReturnedAddress =
                            Substring(local.ReturnAddress.ReturnedAddress, 2,
                            233);
                        }
                        else
                        {
                          break;
                        }
                      }
                      while(!Equal(global.Command, "COMMAND"));
                    }

                    local.EmployerAddress.State =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);

                    break;
                  }
                  else
                  {
                    do
                    {
                      if (IsEmpty(Substring(
                        local.ReturnAddress.ReturnedAddress, 1, 1)))
                      {
                        local.ReturnAddress.ReturnedAddress =
                          Substring(local.ReturnAddress.ReturnedAddress, 2, 233);
                          
                        --local.EndPointer.Count;
                      }
                      else
                      {
                        break;
                      }
                    }
                    while(!Equal(global.Command, "COMMAND"));

                    local.EmployerAddress.State =
                      Substring(local.ReturnAddress.ReturnedAddress, 1,
                      local.EndPointer.Count - 1);
                  }

                  break;
                }

                local.ReturnAddress.ReturnedAddress =
                  Substring(local.ReturnAddress.ReturnedAddress,
                  local.EndPointer.Count +
                  1, FplsLocateResponse.ReturnedAddress_MaxLength -
                  local.EndPointer.Count);
              }
              while(!Equal(global.Command, "COMMAND"));

              switch(local.TotalCount.Count)
              {
                case 0:
                  if (local.TotalCount.Count == 0 && IsEmpty
                    (local.EmployerAddress.Street1) && !
                    IsEmpty(local.ReturnAddress.ReturnedAddress))
                  {
                    local.EmployerAddress.Street1 =
                      TrimEnd(local.ReturnAddress.ReturnedAddress);
                  }

                  break;
                case 1:
                  // leave as is, it is not a complete  address, we can possible
                  // get the city and state from the zip code
                  break;
                case 2:
                  // leave as is, it is not a complete  address, we can possible
                  // get the city and state from the zip code
                  break;
                case 3:
                  local.EmployerAddress.State =
                    local.EmployerAddress.Street3 ?? "";
                  local.EmployerAddress.City =
                    local.EmployerAddress.Street2 ?? "";
                  local.EmployerAddress.Street3 = "";
                  local.EmployerAddress.Street2 = "";
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                case 4:
                  local.EmployerAddress.State =
                    local.EmployerAddress.Street4 ?? "";
                  local.EmployerAddress.City =
                    local.EmployerAddress.Street3 ?? "";
                  local.EmployerAddress.Street4 = "";
                  local.EmployerAddress.Street3 = "";
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street1);
                  local.EmployerAddress.Street2 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                case 5:
                  local.EmployerAddress.State = local.EmployerAddress.City ?? ""
                    ;
                  local.EmployerAddress.City =
                    local.EmployerAddress.Street4 ?? "";
                  local.EmployerAddress.Street4 = "";
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street1);
                  local.EmployerAddress.Street2 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.Street3 =
                    TrimEnd(local.EmployerAddress.Street3);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                case 6:
                  local.EmployerAddress.Street1 =
                    TrimEnd(local.EmployerAddress.Street1);
                  local.EmployerAddress.Street2 =
                    TrimEnd(local.EmployerAddress.Street2);
                  local.EmployerAddress.Street3 =
                    TrimEnd(local.EmployerAddress.Street3);
                  local.EmployerAddress.Street4 =
                    TrimEnd(local.EmployerAddress.Street4);
                  local.EmployerAddress.State =
                    TrimEnd(local.EmployerAddress.State);
                  local.EmployerAddress.City =
                    TrimEnd(local.EmployerAddress.City);

                  break;
                default:
                  // this is an error
                  break;
              }

              if (!IsEmpty(local.EmployerAddress.Street1))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street1, 1, 1)))
                  {
                    local.EmployerAddress.Street1 =
                      Substring(local.EmployerAddress.Street1, 2, 24);
                  }
                  else
                  {
                    goto Test49;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test49:

              if (!IsEmpty(local.EmployerAddress.Street2))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street2, 1, 1)))
                  {
                    local.EmployerAddress.Street2 =
                      Substring(local.EmployerAddress.Street2, 2, 24);
                  }
                  else
                  {
                    goto Test50;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test50:

              if (!IsEmpty(local.EmployerAddress.Street3))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street3, 1, 1)))
                  {
                    local.EmployerAddress.Street3 =
                      Substring(local.EmployerAddress.Street3, 2, 24);
                  }
                  else
                  {
                    goto Test51;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test51:

              if (!IsEmpty(local.EmployerAddress.Street4))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.Street4, 1, 1)))
                  {
                    local.EmployerAddress.Street4 =
                      Substring(local.EmployerAddress.Street4, 2, 24);
                  }
                  else
                  {
                    goto Test52;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test52:

              if (!IsEmpty(local.EmployerAddress.City))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.City, 1, 1)))
                  {
                    local.EmployerAddress.City =
                      Substring(local.EmployerAddress.City, 2, 14);
                  }
                  else
                  {
                    goto Test53;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test53:

              if (!IsEmpty(local.EmployerAddress.State))
              {
                do
                {
                  if (IsEmpty(Substring(local.EmployerAddress.State, 1, 1)))
                  {
                    local.EmployerAddress.State =
                      Substring(local.EmployerAddress.State, 2, 1);
                  }
                  else
                  {
                    goto Test54;
                  }
                }
                while(!Equal(global.Command, "COMMAND"));
              }

Test54:

              local.EmployerAddress.ZipCode =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 198, 5);
              local.EmployerAddress.Zip4 =
                Substring(import.ExternalFplsResponse.ReturnedAddress, 203, 4);
            }
          }
          else
          {
            // ***********************************************************************************************
            // If they send us a employee address we not process the record at 
            // all. This is per business rules.
            // ***********************************************************************************************
            goto Test55;
          }

          if (IsEmpty(local.Employer.Name) || IsEmpty
            (local.EmployerAddress.Street1))
          {
            // ***********************************************************************************************
            // If employer address is not sent then we will not process the 
            // record at all. This is per business rules.
            // ***********************************************************************************************
            goto Test55;
          }

          // ***********************************************************************************************
          // Action Block Maintain Employer will determine if the address is 
          // suitable
          // for automatically generating an Income Witholding Order.
          // ***********************************************************************************************
          local.Employer.Ein = import.ExternalFplsResponse.Fein;
          UseSiB273MaintainEmployer();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (ReadProgramProcessingInfo())
          {
            if (CharAt(entities.ProgramProcessingInfo.ParameterList, 1) == 'Y')
            {
              local.AutomaticGenerateIwo.Flag = "Y";
            }
          }

          if (ReadEmployment())
          {
            // ***********************************************************************************************
            // This dod_date_death_or_separaton is actually the termaination 
            // date for a nsa
            // record. Instead of adding a new field we just piggy backed on 
            // something that
            // was already part of the entity.
            // ***********************************************************************************************
            if (Lt(entities.Employment.EndDt,
              import.ExternalFplsResponse.DateOfHire))
            {
              goto Read;

              // create a new one
            }

            if (Lt(local.NullDate.Date,
              import.ExternalFplsResponse.DodDateOfDeathOrSeparation))
            {
              local.Employment.Assign(entities.Employment);
              local.Employment.EndDt =
                import.ExternalFplsResponse.DodDateOfDeathOrSeparation;
              local.Employment.ReturnDt =
                import.ProgramProcessingInfo.ProcessDate;
              local.Employment.ReturnCd = "O";
              UseSiIncsUpdateIncomeSource();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              UseLeCancelAutoIwoDocuments();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              goto Test55;
            }

            if (!IsEmpty(entities.Employment.ReturnCd) && AsChar
              (entities.Employment.ReturnCd) != 'E')
            {
              // ***********************************************************************************************
              // The return code indicates the employee is not employed at this
              // employer as of the return date, regardless of whether the
              // employment has been end dated.  The employee may have been
              // re-hired.  In this situation, a new employment must be created.
              // ***********************************************************************************************
              goto Read;
            }
            else
            {
              local.NewHireIndicator.Flag = "N";
              local.Employment.Identifier = entities.Employment.Identifier;
              local.Employment.ReturnCd = "E";
              local.Employment.ReturnDt =
                import.ProgramProcessingInfo.ProcessDate;
              local.Employment.Note =
                "Start Date has been supplied by National Security Agency";
              UseSiCabHireDateAlertIwo();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              UseSiB274UpdateEmploymntIncSrc();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (AsChar(local.TenPercentIncrease.Flag) == 'Y')
              {
                local.Infrastructure.ReasonCode = "TENNSAWAGE";
                local.Infrastructure.Detail =
                  "Ten Percent or more Pay Increase from Employer: " + (
                    local.Employer.Name ?? "");
                UseSiB274SendAlertNewIncSrce();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
            }

            goto Test55;
          }

Read:

          if (Lt(local.NullDate.Date,
            import.ExternalFplsResponse.DodDateOfDeathOrSeparation))
          {
            local.Employment.EndDt =
              import.ExternalFplsResponse.DodDateOfDeathOrSeparation;
          }
          else
          {
            local.Employment.EndDt = local.Max.Date;
          }

          local.Employment.StartDt = import.ExternalFplsResponse.DateOfHire;
          local.Employment.Name = local.Employer.Name ?? "";

          // -- 02/15/2010  GVandy CQ6659 NSA employment is considered "verified
          // employment".
          //    Set return code to "E"mployed and the return date to processing 
          // date.
          local.Employment.ReturnCd = "E";
          local.Employment.ReturnDt = import.ProgramProcessingInfo.ProcessDate;
          UseSiB273CreateEmpIncomeSource();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // ***********************************************************************************************
          // After employment has been created  we have to read it so the 
          // identifier can be
          // past on to other cabs.
          // ***********************************************************************************************
          if (ReadEmployment())
          {
            local.Employment.Identifier = entities.Employment.Identifier;
          }

          if (Lt(AddYears(import.ProgramProcessingInfo.ProcessDate, -1),
            import.ExternalFplsResponse.DodDateOfDeathOrSeparation) || !
            Lt(local.NullDate.Date,
            import.ExternalFplsResponse.DodDateOfDeathOrSeparation))
          {
            local.Infrastructure.ReasonCode = "NSAWAGE";

            if (!IsEmpty(local.Employer.Name))
            {
              local.Infrastructure.Detail = "Employer: " + (
                local.Employer.Name ?? "");
            }
            else
            {
              local.Infrastructure.Detail =
                Spaces(Infrastructure.Detail_MaxLength);
            }

            UseSiB274SendAlertNewIncSrce();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
      }

Test55:

      local.FplsRequestFound.Flag = "N";
      local.Next.Identifier = 1;

      if (ReadFplsLocateRequest())
      {
        local.FplsRequestFound.Flag = "Y";

        if (AsChar(entities.FplsLocateRequest.TransactionStatus) == 'R')
        {
          local.FplsLocateRequest.Assign(entities.FplsLocateRequest);

          if (ReadFplsLocateResponse())
          {
            local.Next.Identifier = entities.FplsLocateResponse.Identifier + 1;
          }
        }
        else
        {
          try
          {
            UpdateFplsLocateRequest();
            local.FplsLocateRequest.Assign(entities.FplsLocateRequest);
            ++export.FplsRequestsUpdated.Count;
            ++export.DatabaseActivity.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0000_FPLS_REQUEST_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OE0000_FPLS_REQUEST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      if (AsChar(local.FplsRequestFound.Flag) == 'N')
      {
        local.FplsLocateRequest.Identifier = 1;
        local.FplsLocateRequest.Ssn = local.FplsLocateResponse.SsnReturned ?? ""
          ;
        local.FplsLocateRequest.RequestSentDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.FplsLocateRequest.ApDateOfBirth =
          local.FplsLocateResponse.DodDateOfBirth;
        local.FplsLocateRequest.CaseId =
          import.ExternalFplsResponse.ApCsePersonNumber + NumberToString
          (local.FplsLocateRequest.Identifier, 15);
        local.FplsLocateRequest.UsersField = "Y";
        local.FplsLocateRequest.TransactionStatus = "R";
        local.FplsLocateRequest.StateAbbr = "KS";
        local.FplsLocateRequest.StationNumber = "02";
        local.FplsLocateRequest.TransactionType = "A";
        local.FplsLocateRequest.CreatedBy = import.ProgramProcessingInfo.Name;
        local.FplsLocateRequest.LastUpdatedBy =
          import.ProgramProcessingInfo.Name;
        UseSiB276CreateFplsLocateReqst();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.FplsRequestsCreated.Count;
        ++export.DatabaseActivity.Count;
      }

      if (Equal(import.ExternalFplsResponse.AgencyCode, "A03"))
      {
        return;
      }

      local.FplsLocateResponse.Identifier = local.Next.Identifier;
      local.FplsLocateResponse.DateReceived =
        import.ProgramProcessingInfo.ProcessDate;
      local.FplsLocateResponse.UsageStatus = "U";
      local.FplsLocateResponse.DateUsed = local.NullDate.Date;
      local.FplsLocateResponse.CreatedBy = import.ProgramProcessingInfo.Name;
      local.FplsLocateResponse.LastUpdatedBy =
        import.ProgramProcessingInfo.Name;
      UseSiB276CreateFplsResponse();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++export.FplsResponsesCreated.Count;
      ++export.DatabaseActivity.Count;

      if (Equal(import.ExternalFplsResponse.ResponseCode, "10"))
      {
        // ********************************************************
        // Code indicates no information available on this person.
        // Therefore, no need to alert the worker.
        // ********************************************************
        return;
      }

      // ***	Insert Event for FPLS SESA tape received	***
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.EventId = 10;
      local.Infrastructure.UserId = "FPLS";
      local.Infrastructure.ReasonCode = "FPLSRCV";
      local.Infrastructure.BusinessObjectCd = "FPL";
      local.Infrastructure.DenormNumeric12 = local.FplsLocateRequest.Identifier;
      local.Infrastructure.ReferenceDate =
        import.ProgramProcessingInfo.ProcessDate;
      local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;
      local.Infrastructure.LastUpdatedBy = import.ProgramProcessingInfo.Name;
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;

      // *****************************************************************************
      // CQ7536 4/28/10  LSS
      // Check for an existing response to set the date from.
      // If there is no existing response, use the received date of the new 
      // response.
      // *****************************************************************************
      if (entities.FplsLocateResponse.Populated)
      {
        local.ConvertDateDateWorkArea.Date =
          entities.FplsLocateResponse.DateReceived;
      }
      else
      {
        local.ConvertDateDateWorkArea.Date =
          local.FplsLocateResponse.DateReceived;
      }

      UseCabConvertDate2String();

      foreach(var item in ReadInfrastructure())
      {
        if (Equal(entities.Infrastructure.ReferenceDate,
          local.Infrastructure.ReferenceDate))
        {
          return;
        }
      }

      if (Equal(local.FplsLocateResponse.AgencyCode, "B01") || Equal
        (local.FplsLocateResponse.AgencyCode, "H99"))
      {
        local.Infrastructure.Detail =
          "FPLS New Hire Address Information Received.";
      }
      else
      {
        local.Infrastructure.Detail = "FPLS Locate Response Received Date" + " :   " +
          local.ConvertDateTextWorkArea.Text8;

        // *****************************************************************************
        // CQ7536 4/28/10  LSS
        // Get the Agency Code from the new response not an existing response.
        // *****************************************************************************
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "    From Agency :   " +
          (local.FplsLocateResponse.AgencyCode ?? "");
      }

      local.Infrastructure.ProcessStatus = "Q";

      foreach(var item in ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        if (ReadInterstateRequest())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "OS";
          }
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.FplsAlertsCreated.Count;
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        import.ExternalFplsResponse.ApCsePersonNumber + " Identifier:" + NumberToString
        (import.ExternalFplsResponse.FplsRequestIdentifier, 15) + "FPLS LOAD : CSE Person Not Found.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveExternalFplsResponse(ExternalFplsResponse source,
    ExternalFplsResponse target)
  {
    target.AgencyCode = source.AgencyCode;
    target.ApNameReturned = source.ApNameReturned;
  }

  private static void MoveFplsLocateRequest(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
    target.RequestSentDate = source.RequestSentDate;
    target.StateAbbr = source.StateAbbr;
    target.StationNumber = source.StationNumber;
    target.TransactionType = source.TransactionType;
    target.Ssn = source.Ssn;
    target.CaseId = source.CaseId;
    target.LocalCode = source.LocalCode;
    target.UsersField = source.UsersField;
    target.TypeOfCase = source.TypeOfCase;
    target.ApFirstName = source.ApFirstName;
    target.ApMiddleName = source.ApMiddleName;
    target.ApFirstLastName = source.ApFirstLastName;
    target.ApSecondLastName = source.ApSecondLastName;
    target.ApThirdLastName = source.ApThirdLastName;
    target.ApDateOfBirth = source.ApDateOfBirth;
    target.Sex = source.Sex;
    target.CollectAllResponsesTogether = source.CollectAllResponsesTogether;
    target.TransactionError = source.TransactionError;
    target.ApCityOfBirth = source.ApCityOfBirth;
    target.ApStateOrCountryOfBirth = source.ApStateOrCountryOfBirth;
    target.ApsFathersFirstName = source.ApsFathersFirstName;
    target.ApsFathersMi = source.ApsFathersMi;
    target.ApsFathersLastName = source.ApsFathersLastName;
    target.ApsMothersFirstName = source.ApsMothersFirstName;
    target.ApsMothersMi = source.ApsMothersMi;
    target.ApsMothersMaidenName = source.ApsMothersMaidenName;
    target.CpSsn = source.CpSsn;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.SendRequestTo = source.SendRequestTo;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.LastQtrIncome = source.LastQtrIncome;
    target.LastQtr = source.LastQtr;
    target.LastQtrYr = source.LastQtrYr;
    target.Attribute2NdQtrIncome = source.Attribute2NdQtrIncome;
    target.Attribute2NdQtr = source.Attribute2NdQtr;
    target.Attribute2NdQtrYr = source.Attribute2NdQtrYr;
    target.Attribute3RdQtrIncome = source.Attribute3RdQtrIncome;
    target.Attribute3RdQtr = source.Attribute3RdQtr;
    target.Attribute3RdQtrYr = source.Attribute3RdQtrYr;
    target.Attribute4ThQtrIncome = source.Attribute4ThQtrIncome;
    target.Attribute4ThQtr = source.Attribute4ThQtr;
    target.Attribute4ThQtrYr = source.Attribute4ThQtrYr;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.WorkerId = source.WorkerId;
    target.Note = source.Note;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource3(IncomeSource source, IncomeSource target)
    
  {
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.Note = source.Note;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Detail = source.Detail;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ConvertDateDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ConvertDateTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabProcessDateOfDeath()
  {
    var useImport = new CabProcessDateOfDeath.Import();
    var useExport = new CabProcessDateOfDeath.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    MoveExternalFplsResponse(import.ExternalFplsResponse,
      useImport.ExternalFplsResponse);
    useImport.SsaStateLastResidence.Text2 = local.NsaStateLastResidence.Text2;
    useImport.SsaCityLastResidence.Text15 = local.NsaCityLastResidence.Text15;
    useImport.FcrPersonAckErrorRecord.Assign(local.FcrPersonAckErrorRecord);
    useImport.DodAlertsCreated.Count = local.AlertsCreated.Count;
    useImport.DodEventsCreated.Count = local.EventsCreated.Count;
    useImport.PersonsUpdated.Count = local.PersonsUpdated.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.ConvertDateOfDeath.Text8 = local.ConvertDateTextWorkArea.Text8;
    useExport.DodAlertsCreated.Count = local.AlertsCreated.Count;

    Call(CabProcessDateOfDeath.Execute, useImport, useExport);

    local.PersonUpdated.Count = useExport.PersonsUpdated.Count;
    local.AlertsCreated.Count = useExport.DodAlertsCreated.Count;
    local.EventsCreated.Count = useExport.DodEventsCreated.Count;
  }

  private void UseLeCancelAutoIwoDocuments()
  {
    var useImport = new LeCancelAutoIwoDocuments.Import();
    var useExport = new LeCancelAutoIwoDocuments.Export();

    useImport.IncomeSource.Identifier = local.Employment.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(LeCancelAutoIwoDocuments.Execute, useImport, useExport);
  }

  private void UseSiB273CreateEmpIncomeSource()
  {
    var useImport = new SiB273CreateEmpIncomeSource.Import();
    var useExport = new SiB273CreateEmpIncomeSource.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.Employment.Assign(local.Employment);
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;
    useImport.Employer.Identifier = local.Employer.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiB273CreateEmpIncomeSource.Execute, useImport, useExport);
  }

  private void UseSiB273MaintainEmployer()
  {
    var useImport = new SiB273MaintainEmployer.Import();
    var useExport = new SiB273MaintainEmployer.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.Employer.Assign(local.Employer);

    Call(SiB273MaintainEmployer.Execute, useImport, useExport);

    local.AddressSuitableForIwo.Flag = useExport.AddressSuitableForIwo.Flag;

    local.Employer.Assign(useExport.Employer);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB274SendAlertNewIncSrce()
  {
    var useImport = new SiB274SendAlertNewIncSrce.Import();
    var useExport = new SiB274SendAlertNewIncSrce.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.Employment.Identifier = local.Employment.Identifier;
    useImport.Process.Date = local.DetermineQtr.Date;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SiB274SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private void UseSiB274UpdateEmploymntIncSrc()
  {
    var useImport = new SiB274UpdateEmploymntIncSrc.Import();
    var useExport = new SiB274UpdateEmploymntIncSrc.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.Wage.AverageCurrency = local.Wages.AverageCurrency;
    useImport.Quarter.Text1 = local.Quater.Text1;
    useImport.Employment.Identifier = local.Employment.Identifier;
    useImport.Employer.Name = local.Employer.Name;
    useImport.Year.Year = local.DetermineQtr.Year;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Process.Date = local.Current.Date;

    Call(SiB274UpdateEmploymntIncSrc.Execute, useImport, useExport);

    local.TenPercentIncrease.Flag = useExport.TenPercentIncrease.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB276CreateFplsLocateReqst()
  {
    var useImport = new SiB276CreateFplsLocateReqst.Import();
    var useExport = new SiB276CreateFplsLocateReqst.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveFplsLocateRequest(local.FplsLocateRequest, useImport.FplsLocateRequest);

    Call(SiB276CreateFplsLocateReqst.Execute, useImport, useExport);
  }

  private void UseSiB276CreateFplsResponse()
  {
    var useImport = new SiB276CreateFplsResponse.Import();
    var useExport = new SiB276CreateFplsResponse.Export();

    useImport.FplsLocateResponse.Assign(local.FplsLocateResponse);
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.FplsLocateRequest.Identifier = local.FplsLocateRequest.Identifier;

    Call(SiB276CreateFplsResponse.Execute, useImport, useExport);
  }

  private void UseSiCabHireDateAlertIwo()
  {
    var useImport = new SiCabHireDateAlertIwo.Import();
    var useExport = new SiCabHireDateAlertIwo.Export();

    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;
    useImport.FederalNewhireIndicator.Flag = local.NewHireIndicator.Flag;
    useImport.DateOfHireUpdates.Count = local.DateOfHireUpdates.Count;
    MoveIncomeSource3(local.Employment, useImport.NewInfo);
    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    useImport.DateOfHire.Date = local.DateOfHire.Date;
    MoveEmployer(local.Employer, useImport.Employer);
    useImport.Employment.Identifier = entities.Employment.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Process.Date = local.Current.Date;
    useImport.FplsLocateResponse.AgencyCode =
      local.FplsLocateResponse.AgencyCode;

    Call(SiCabHireDateAlertIwo.Execute, useImport, useExport);

    local.PersonsUpdated.Count = useExport.DateOfHireUpdates.Count;
  }

  private void UseSiIncsUpdateIncomeSource()
  {
    var useImport = new SiIncsUpdateIncomeSource.Import();
    var useExport = new SiIncsUpdateIncomeSource.Export();

    MoveIncomeSource2(local.Employment, useImport.IncomeSource);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.CsePerson.Assign(local.CsePerson);

    Call(SiIncsUpdateIncomeSource.Execute, useImport, useExport);

    MoveIncomeSource1(useImport.IncomeSource, local.Employment);
    local.CsePerson.Assign(useImport.CsePerson);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", import.ExternalFplsResponse.ApCsePersonNumber);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.ExternalFplsResponse.ApCsePersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadEmployment()
  {
    entities.Employment.Populated = false;

    return Read("ReadEmployment",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableString(command, "ein", import.ExternalFplsResponse.Fein);
      },
      (db, reader) =>
      {
        entities.Employment.Identifier = db.GetDateTime(reader, 0);
        entities.Employment.Type1 = db.GetString(reader, 1);
        entities.Employment.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.Employment.LastQtr = db.GetNullableString(reader, 3);
        entities.Employment.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.Employment.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.Employment.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.Employment.Attribute2NdQtrYr = db.GetNullableInt32(reader, 7);
        entities.Employment.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.Employment.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.Employment.Attribute3RdQtrYr = db.GetNullableInt32(reader, 10);
        entities.Employment.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.Employment.Attribute4ThQtr = db.GetNullableString(reader, 12);
        entities.Employment.Attribute4ThQtrYr = db.GetNullableInt32(reader, 13);
        entities.Employment.SentDt = db.GetNullableDate(reader, 14);
        entities.Employment.ReturnDt = db.GetNullableDate(reader, 15);
        entities.Employment.ReturnCd = db.GetNullableString(reader, 16);
        entities.Employment.Name = db.GetNullableString(reader, 17);
        entities.Employment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.Employment.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Employment.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.Employment.CreatedBy = db.GetString(reader, 21);
        entities.Employment.CspINumber = db.GetString(reader, 22);
        entities.Employment.SelfEmployedInd = db.GetNullableString(reader, 23);
        entities.Employment.EmpId = db.GetNullableInt32(reader, 24);
        entities.Employment.SendTo = db.GetNullableString(reader, 25);
        entities.Employment.WorkerId = db.GetNullableString(reader, 26);
        entities.Employment.StartDt = db.GetNullableDate(reader, 27);
        entities.Employment.EndDt = db.GetNullableDate(reader, 28);
        entities.Employment.Note = db.GetNullableString(reader, 29);
        entities.Employment.Note2 = db.GetNullableString(reader, 30);
        entities.Employment.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Employment.Type1);
        CheckValid<IncomeSource>("SendTo", entities.Employment.SendTo);
      });
  }

  private bool ReadFplsLocateRequest()
  {
    entities.FplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.FplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.FplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.FplsLocateRequest.StateAbbr = db.GetNullableString(reader, 3);
        entities.FplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 4);
        entities.FplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 5);
        entities.FplsLocateRequest.Ssn = db.GetNullableString(reader, 6);
        entities.FplsLocateRequest.CaseId = db.GetNullableString(reader, 7);
        entities.FplsLocateRequest.LocalCode = db.GetNullableString(reader, 8);
        entities.FplsLocateRequest.UsersField = db.GetNullableString(reader, 9);
        entities.FplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 10);
        entities.FplsLocateRequest.ApFirstName =
          db.GetNullableString(reader, 11);
        entities.FplsLocateRequest.ApMiddleName =
          db.GetNullableString(reader, 12);
        entities.FplsLocateRequest.ApFirstLastName =
          db.GetNullableString(reader, 13);
        entities.FplsLocateRequest.ApSecondLastName =
          db.GetNullableString(reader, 14);
        entities.FplsLocateRequest.ApThirdLastName =
          db.GetNullableString(reader, 15);
        entities.FplsLocateRequest.ApDateOfBirth =
          db.GetNullableDate(reader, 16);
        entities.FplsLocateRequest.Sex = db.GetNullableString(reader, 17);
        entities.FplsLocateRequest.CollectAllResponsesTogether =
          db.GetNullableString(reader, 18);
        entities.FplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 19);
        entities.FplsLocateRequest.ApCityOfBirth =
          db.GetNullableString(reader, 20);
        entities.FplsLocateRequest.ApStateOrCountryOfBirth =
          db.GetNullableString(reader, 21);
        entities.FplsLocateRequest.ApsFathersFirstName =
          db.GetNullableString(reader, 22);
        entities.FplsLocateRequest.ApsFathersMi =
          db.GetNullableString(reader, 23);
        entities.FplsLocateRequest.ApsFathersLastName =
          db.GetNullableString(reader, 24);
        entities.FplsLocateRequest.ApsMothersFirstName =
          db.GetNullableString(reader, 25);
        entities.FplsLocateRequest.ApsMothersMi =
          db.GetNullableString(reader, 26);
        entities.FplsLocateRequest.ApsMothersMaidenName =
          db.GetNullableString(reader, 27);
        entities.FplsLocateRequest.CpSsn = db.GetNullableString(reader, 28);
        entities.FplsLocateRequest.CreatedBy = db.GetString(reader, 29);
        entities.FplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 30);
        entities.FplsLocateRequest.LastUpdatedBy = db.GetString(reader, 31);
        entities.FplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 32);
        entities.FplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 33);
        entities.FplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 34);
        entities.FplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse()
  {
    entities.FplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.
          SetInt32(command, "flqIdentifier", local.FplsLocateRequest.Identifier);
          
      },
      (db, reader) =>
      {
        entities.FplsLocateResponse.FlqIdentifier = db.GetInt32(reader, 0);
        entities.FplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.FplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.FplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.FplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.FplsLocateResponse.DateUsed = db.GetNullableDate(reader, 5);
        entities.FplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.FplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.FplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.FplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.FplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.FplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.FplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.FplsLocateResponse.DodEligibilityCode =
          db.GetNullableString(reader, 13);
        entities.FplsLocateResponse.DodDateOfDeathOrSeparation =
          db.GetNullableDate(reader, 14);
        entities.FplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 15);
        entities.FplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 16);
        entities.FplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 17);
        entities.FplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 18);
        entities.FplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 19);
        entities.FplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 20);
        entities.FplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 21);
        entities.FplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 22);
        entities.FplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 23);
        entities.FplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 24);
        entities.FplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 25);
        entities.FplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 26);
        entities.FplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.FplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 28);
        entities.FplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 29);
        entities.FplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 30);
        entities.FplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 31);
        entities.FplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 32);
        entities.FplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 33);
        entities.FplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 34);
        entities.FplsLocateResponse.CreatedBy =
          db.GetNullableString(reader, 35);
        entities.FplsLocateResponse.CreatedTimestamp =
          db.GetNullableDateTime(reader, 36);
        entities.FplsLocateResponse.LastUpdatedBy =
          db.GetNullableString(reader, 37);
        entities.FplsLocateResponse.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 38);
        entities.FplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 39);
        entities.FplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 40);
        entities.FplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 41);
        entities.FplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 42);
        entities.FplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 43);
        entities.FplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 44);
        entities.FplsLocateResponse.HealthInsBenefitIndicator =
          db.GetNullableString(reader, 45);
        entities.FplsLocateResponse.EmploymentStatus =
          db.GetNullableString(reader, 46);
        entities.FplsLocateResponse.EmploymentInd =
          db.GetNullableString(reader, 47);
        entities.FplsLocateResponse.DateOfHire = db.GetNullableDate(reader, 48);
        entities.FplsLocateResponse.ReportingFedAgency =
          db.GetNullableString(reader, 49);
        entities.FplsLocateResponse.Fein = db.GetNullableString(reader, 50);
        entities.FplsLocateResponse.CorrectedAdditionMultipleSsn =
          db.GetNullableString(reader, 51);
        entities.FplsLocateResponse.SsnMatchInd =
          db.GetNullableString(reader, 52);
        entities.FplsLocateResponse.ReportingQuarter =
          db.GetNullableString(reader, 53);
        entities.FplsLocateResponse.NdnhResponse =
          db.GetNullableString(reader, 54);
        entities.FplsLocateResponse.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum", local.Infrastructure.CsePersonNumber ?? "");
        db.SetInt32(command, "eventId", local.Infrastructure.EventId);
        db.SetString(command, "userId", local.Infrastructure.UserId);
        db.SetString(command, "reasonCode", local.Infrastructure.ReasonCode);
        db.SetString(command, "createdBy", local.Infrastructure.CreatedBy);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.Infrastructure.UserId = db.GetString(reader, 4);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 5);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 6);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", import.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.FplsLocateRequest.Populated);

    var transactionStatus = "R";
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();
    var sendRequestTo = local.FplsLocateRequest.SendRequestTo ?? "";

    entities.FplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
        db.
          SetString(command, "cspNumber", entities.FplsLocateRequest.CspNumber);
          
        db.
          SetInt32(command, "identifier", entities.FplsLocateRequest.Identifier);
          
      });

    entities.FplsLocateRequest.TransactionStatus = transactionStatus;
    entities.FplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.FplsLocateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.FplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.FplsLocateRequest.Populated = true;
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
    /// A value of DatabaseActivity.
    /// </summary>
    [JsonPropertyName("databaseActivity")]
    public Common DatabaseActivity
    {
      get => databaseActivity ??= new();
      set => databaseActivity = value;
    }

    /// <summary>
    /// A value of FplsResponsesCreated.
    /// </summary>
    [JsonPropertyName("fplsResponsesCreated")]
    public Common FplsResponsesCreated
    {
      get => fplsResponsesCreated ??= new();
      set => fplsResponsesCreated = value;
    }

    /// <summary>
    /// A value of FplsRequestsUpdated.
    /// </summary>
    [JsonPropertyName("fplsRequestsUpdated")]
    public Common FplsRequestsUpdated
    {
      get => fplsRequestsUpdated ??= new();
      set => fplsRequestsUpdated = value;
    }

    /// <summary>
    /// A value of FplsRequestsCreated.
    /// </summary>
    [JsonPropertyName("fplsRequestsCreated")]
    public Common FplsRequestsCreated
    {
      get => fplsRequestsCreated ??= new();
      set => fplsRequestsCreated = value;
    }

    /// <summary>
    /// A value of FplsResponsesSkipped.
    /// </summary>
    [JsonPropertyName("fplsResponsesSkipped")]
    public Common FplsResponsesSkipped
    {
      get => fplsResponsesSkipped ??= new();
      set => fplsResponsesSkipped = value;
    }

    /// <summary>
    /// A value of FplsAlertsCreated.
    /// </summary>
    [JsonPropertyName("fplsAlertsCreated")]
    public Common FplsAlertsCreated
    {
      get => fplsAlertsCreated ??= new();
      set => fplsAlertsCreated = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ExternalFplsResponse.
    /// </summary>
    [JsonPropertyName("externalFplsResponse")]
    public ExternalFplsResponse ExternalFplsResponse
    {
      get => externalFplsResponse ??= new();
      set => externalFplsResponse = value;
    }

    /// <summary>
    /// A value of DateOfDeathIndicator.
    /// </summary>
    [JsonPropertyName("dateOfDeathIndicator")]
    public TextWorkArea DateOfDeathIndicator
    {
      get => dateOfDeathIndicator ??= new();
      set => dateOfDeathIndicator = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private Common databaseActivity;
    private Common fplsResponsesCreated;
    private Common fplsRequestsUpdated;
    private Common fplsRequestsCreated;
    private Common fplsResponsesSkipped;
    private Common fplsAlertsCreated;
    private ProgramProcessingInfo programProcessingInfo;
    private ExternalFplsResponse externalFplsResponse;
    private TextWorkArea dateOfDeathIndicator;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FplsResponsesSkipped.
    /// </summary>
    [JsonPropertyName("fplsResponsesSkipped")]
    public Common FplsResponsesSkipped
    {
      get => fplsResponsesSkipped ??= new();
      set => fplsResponsesSkipped = value;
    }

    /// <summary>
    /// A value of FplsAlertsCreated.
    /// </summary>
    [JsonPropertyName("fplsAlertsCreated")]
    public Common FplsAlertsCreated
    {
      get => fplsAlertsCreated ??= new();
      set => fplsAlertsCreated = value;
    }

    /// <summary>
    /// A value of DatabaseActivity.
    /// </summary>
    [JsonPropertyName("databaseActivity")]
    public Common DatabaseActivity
    {
      get => databaseActivity ??= new();
      set => databaseActivity = value;
    }

    /// <summary>
    /// A value of FplsResponsesCreated.
    /// </summary>
    [JsonPropertyName("fplsResponsesCreated")]
    public Common FplsResponsesCreated
    {
      get => fplsResponsesCreated ??= new();
      set => fplsResponsesCreated = value;
    }

    /// <summary>
    /// A value of FplsRequestsUpdated.
    /// </summary>
    [JsonPropertyName("fplsRequestsUpdated")]
    public Common FplsRequestsUpdated
    {
      get => fplsRequestsUpdated ??= new();
      set => fplsRequestsUpdated = value;
    }

    /// <summary>
    /// A value of FplsRequestsCreated.
    /// </summary>
    [JsonPropertyName("fplsRequestsCreated")]
    public Common FplsRequestsCreated
    {
      get => fplsRequestsCreated ??= new();
      set => fplsRequestsCreated = value;
    }

    private Common fplsResponsesSkipped;
    private Common fplsAlertsCreated;
    private Common databaseActivity;
    private Common fplsResponsesCreated;
    private Common fplsRequestsUpdated;
    private Common fplsRequestsCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of NsaStateLastResidence.
    /// </summary>
    [JsonPropertyName("nsaStateLastResidence")]
    public WorkArea NsaStateLastResidence
    {
      get => nsaStateLastResidence ??= new();
      set => nsaStateLastResidence = value;
    }

    /// <summary>
    /// A value of NsaCityLastResidence.
    /// </summary>
    [JsonPropertyName("nsaCityLastResidence")]
    public WorkArea NsaCityLastResidence
    {
      get => nsaCityLastResidence ??= new();
      set => nsaCityLastResidence = value;
    }

    /// <summary>
    /// A value of BeginQuarter.
    /// </summary>
    [JsonPropertyName("beginQuarter")]
    public DateWorkArea BeginQuarter
    {
      get => beginQuarter ??= new();
      set => beginQuarter = value;
    }

    /// <summary>
    /// A value of FpositionCount.
    /// </summary>
    [JsonPropertyName("fpositionCount")]
    public Common FpositionCount
    {
      get => fpositionCount ??= new();
      set => fpositionCount = value;
    }

    /// <summary>
    /// A value of ReturnAddress.
    /// </summary>
    [JsonPropertyName("returnAddress")]
    public FplsLocateResponse ReturnAddress
    {
      get => returnAddress ??= new();
      set => returnAddress = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of EndPointer.
    /// </summary>
    [JsonPropertyName("endPointer")]
    public Common EndPointer
    {
      get => endPointer ??= new();
      set => endPointer = value;
    }

    /// <summary>
    /// A value of DeleteMe.
    /// </summary>
    [JsonPropertyName("deleteMe")]
    public ProgramProcessingInfo DeleteMe
    {
      get => deleteMe ??= new();
      set => deleteMe = value;
    }

    /// <summary>
    /// A value of Wages.
    /// </summary>
    [JsonPropertyName("wages")]
    public Common Wages
    {
      get => wages ??= new();
      set => wages = value;
    }

    /// <summary>
    /// A value of CurrentMonth.
    /// </summary>
    [JsonPropertyName("currentMonth")]
    public Common CurrentMonth
    {
      get => currentMonth ??= new();
      set => currentMonth = value;
    }

    /// <summary>
    /// A value of LessThanAYear.
    /// </summary>
    [JsonPropertyName("lessThanAYear")]
    public Common LessThanAYear
    {
      get => lessThanAYear ??= new();
      set => lessThanAYear = value;
    }

    /// <summary>
    /// A value of HiredMonth.
    /// </summary>
    [JsonPropertyName("hiredMonth")]
    public Common HiredMonth
    {
      get => hiredMonth ??= new();
      set => hiredMonth = value;
    }

    /// <summary>
    /// A value of TenPercentIncrease.
    /// </summary>
    [JsonPropertyName("tenPercentIncrease")]
    public Common TenPercentIncrease
    {
      get => tenPercentIncrease ??= new();
      set => tenPercentIncrease = value;
    }

    /// <summary>
    /// A value of YearHired.
    /// </summary>
    [JsonPropertyName("yearHired")]
    public DateWorkArea YearHired
    {
      get => yearHired ??= new();
      set => yearHired = value;
    }

    /// <summary>
    /// A value of Quater.
    /// </summary>
    [JsonPropertyName("quater")]
    public TextWorkArea Quater
    {
      get => quater ??= new();
      set => quater = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    /// <summary>
    /// A value of DateOfHire.
    /// </summary>
    [JsonPropertyName("dateOfHire")]
    public DateWorkArea DateOfHire
    {
      get => dateOfHire ??= new();
      set => dateOfHire = value;
    }

    /// <summary>
    /// A value of DateOfHireUpdates.
    /// </summary>
    [JsonPropertyName("dateOfHireUpdates")]
    public Common DateOfHireUpdates
    {
      get => dateOfHireUpdates ??= new();
      set => dateOfHireUpdates = value;
    }

    /// <summary>
    /// A value of NewHireIndicator.
    /// </summary>
    [JsonPropertyName("newHireIndicator")]
    public Common NewHireIndicator
    {
      get => newHireIndicator ??= new();
      set => newHireIndicator = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of DetermineQtr.
    /// </summary>
    [JsonPropertyName("determineQtr")]
    public DateWorkArea DetermineQtr
    {
      get => determineQtr ??= new();
      set => determineQtr = value;
    }

    /// <summary>
    /// A value of DetermineSalary.
    /// </summary>
    [JsonPropertyName("determineSalary")]
    public Common DetermineSalary
    {
      get => determineSalary ??= new();
      set => determineSalary = value;
    }

    /// <summary>
    /// A value of RecordUpdated.
    /// </summary>
    [JsonPropertyName("recordUpdated")]
    public Common RecordUpdated
    {
      get => recordUpdated ??= new();
      set => recordUpdated = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of FplsSourceName.
    /// </summary>
    [JsonPropertyName("fplsSourceName")]
    public TextWorkArea FplsSourceName
    {
      get => fplsSourceName ??= new();
      set => fplsSourceName = value;
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

    /// <summary>
    /// A value of FcrPersonAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrPersonAckErrorRecord")]
    public FcrPersonAckErrorRecord FcrPersonAckErrorRecord
    {
      get => fcrPersonAckErrorRecord ??= new();
      set => fcrPersonAckErrorRecord = value;
    }

    /// <summary>
    /// A value of PersonUpdated.
    /// </summary>
    [JsonPropertyName("personUpdated")]
    public Common PersonUpdated
    {
      get => personUpdated ??= new();
      set => personUpdated = value;
    }

    /// <summary>
    /// A value of ConvertDateOfDeath.
    /// </summary>
    [JsonPropertyName("convertDateOfDeath")]
    public TextWorkArea ConvertDateOfDeath
    {
      get => convertDateOfDeath ??= new();
      set => convertDateOfDeath = value;
    }

    /// <summary>
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
    }

    /// <summary>
    /// A value of EventsCreated.
    /// </summary>
    [JsonPropertyName("eventsCreated")]
    public Common EventsCreated
    {
      get => eventsCreated ??= new();
      set => eventsCreated = value;
    }

    /// <summary>
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
    }

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
    /// A value of ResponseFoundFlag.
    /// </summary>
    [JsonPropertyName("responseFoundFlag")]
    public Common ResponseFoundFlag
    {
      get => responseFoundFlag ??= new();
      set => responseFoundFlag = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of DateError.
    /// </summary>
    [JsonPropertyName("dateError")]
    public Common DateError
    {
      get => dateError ??= new();
      set => dateError = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of DateMmddyyAlpha.
    /// </summary>
    [JsonPropertyName("dateMmddyyAlpha")]
    public DateMmddyyAlpha DateMmddyyAlpha
    {
      get => dateMmddyyAlpha ??= new();
      set => dateMmddyyAlpha = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of FplsRequestFound.
    /// </summary>
    [JsonPropertyName("fplsRequestFound")]
    public Common FplsRequestFound
    {
      get => fplsRequestFound ??= new();
      set => fplsRequestFound = value;
    }

    /// <summary>
    /// A value of NdnhProactiveMatch.
    /// </summary>
    [JsonPropertyName("ndnhProactiveMatch")]
    public Common NdnhProactiveMatch
    {
      get => ndnhProactiveMatch ??= new();
      set => ndnhProactiveMatch = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public FplsLocateResponse Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ConvertDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateDateWorkArea")]
    public DateWorkArea ConvertDateDateWorkArea
    {
      get => convertDateDateWorkArea ??= new();
      set => convertDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateTextWorkArea")]
    public TextWorkArea ConvertDateTextWorkArea
    {
      get => convertDateTextWorkArea ??= new();
      set => convertDateTextWorkArea = value;
    }

    private WorkArea message2;
    private WorkArea message1;
    private SsnWorkArea convert;
    private WorkArea nsaStateLastResidence;
    private WorkArea nsaCityLastResidence;
    private DateWorkArea beginQuarter;
    private Common fpositionCount;
    private FplsLocateResponse returnAddress;
    private Common totalCount;
    private Common endPointer;
    private ProgramProcessingInfo deleteMe;
    private Common wages;
    private Common currentMonth;
    private Common lessThanAYear;
    private Common hiredMonth;
    private Common tenPercentIncrease;
    private DateWorkArea yearHired;
    private TextWorkArea quater;
    private IncomeSource employment;
    private DateWorkArea dateOfHire;
    private Common dateOfHireUpdates;
    private Common newHireIndicator;
    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
    private EmployerAddress employerAddress;
    private Employer employer;
    private DateWorkArea determineQtr;
    private Common determineSalary;
    private Common recordUpdated;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private DateWorkArea blank;
    private TextWorkArea fplsSourceName;
    private DateWorkArea current;
    private FcrPersonAckErrorRecord fcrPersonAckErrorRecord;
    private Common personUpdated;
    private TextWorkArea convertDateOfDeath;
    private Common alertsCreated;
    private Common eventsCreated;
    private Common personsUpdated;
    private DateWorkArea max;
    private Common responseFoundFlag;
    private FplsLocateResponse fplsLocateResponse;
    private FplsLocateRequest fplsLocateRequest;
    private Common dateError;
    private DateWorkArea nullDate;
    private DateMmddyyAlpha dateMmddyyAlpha;
    private DateWorkArea dateWorkArea;
    private Common fplsRequestFound;
    private Common ndnhProactiveMatch;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private FplsLocateResponse next;
    private External passArea;
    private Infrastructure infrastructure;
    private DateWorkArea convertDateDateWorkArea;
    private TextWorkArea convertDateTextWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
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

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private InvalidSsn invalidSsn;
    private ProgramProcessingInfo programProcessingInfo;
    private Employer employer;
    private IncomeSource employment;
    private IncomeSource incomeSource;
    private CaseRole caseRole;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
    private FplsLocateRequest fplsLocateRequest;
    private FplsLocateResponse fplsLocateResponse;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
  }
#endregion
}
