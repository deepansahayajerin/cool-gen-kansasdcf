// Program: SI_IIMC_READ_INCOMING_IS_INFO, ID: 372505526, model: 746.
// Short name: SWE02135
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
/// A program: SI_IIMC_READ_INCOMING_IS_INFO.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiIimcReadIncomingIsInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIMC_READ_INCOMING_IS_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIimcReadIncomingIsInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIimcReadIncomingIsInfo.
  /// </summary>
  public SiIimcReadIncomingIsInfo(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------
    // 10/20/97    Sid Chowdhary	Initial Development.
    // 12/23/98    C Deghand           Changed the order of the reads for
    //                                 
    // the AR and AP and modified the
    //                                 
    // ESCAPE for the AP.
    // 06/12/00    C Scroggins         PR#95574 Made changes to close all 
    // interstate requests
    //                                 
    // open for another State when that
    // State's interstate
    //                                 
    // case is closed.
    // -----------------------------------------------------------
    // 04/09/01 C Fairley I00117417    Changed check from NOT EQUAL "Y" to EQUAL
    // "N".
    //                                 
    // Changed check from EQUAL "Y" to
    // EQUAL "Y" or SPACES
    //                                 
    // Removed commented out code.
    // -----------------------------------------------------------------------------------
    // 06/12/01 C Fairley I00121385    Do not increment counter, when the 
    // int_h_generated_id
    //                                 
    // is equal to the previous
    // int_h_generated_id.
    //                                 
    // Replaced literal(s) with local
    // view(s) in READ
    //                                 
    // statements.
    //                                 
    // Removed commented out code.
    // -----------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------
    // 06/20/01  T Bobb WR 010501      If request is  an
    // 
    // outgoing, check history to see if there is an
    // 
    // incoming that is closed.
    // --------------------------------------------------------------------------------------
    // 09/16/02  T.Bobb  PR00127141 Flow to IREQ when there
    // is one domestic and one Foreign for the same case number.
    // -------------------------------------------------------------------------------
    // 10/21/02 T.Bobb PR00125639 Changed reads of contact,
    // contact address, and payment address back to select
    // only. Previously the program was abending with an 811,
    // duplicate rows being returned. The update program has
    // been modified to not insert duplicate rows.
    // -------------------------------------------------------------------------------
    // >>
    // 11/01/2002 T.Bobb PR00138045
    // Futher qualified read by checking end date equal to case
    // status date. This corrected the problem when there were
    // two AR's that were end dated. The read used to get the
    // wrong one that wasn't associated to the interstate request.
    // -------------------------------------------------------------------------------
    // >>
    // 11/18/2002 T.Bobb PR00123201
    // Set the int_h_generated _id so in the read down below it
    // can find the interstate request when it is a foreign country.
    // -------------------------------------------------------------------------------
    // 05/10/06 GVandy 	 WR230751	Add support for Tribal IV-D agencies.
    // 06/13/18  Harden        CQ62215     Add a foe;d fpr Fax # on IIMC
    local.Current.Date = Now().Date;
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.InterstateCase1.Flag = "N";
    UseOeCabSetMnemonics();
    export.OtherState.Assign(import.OtherState);
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveInterstateRequest1(import.InterstateRequest, export.InterstateRequest);
    export.RetcompInd.Flag = import.RetcompInd.Flag;

    if (ReadCase())
    {
      export.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (AsChar(export.Case1.Status) == 'C')
    {
      local.InterstateRequestCount.Count = 0;
      local.InterstateRequestHistory.Count = 0;

      foreach(var item in ReadInterstateRequest7())
      {
        ++local.InterstateRequestCount.Count;

        // *** Problem report I00117417
        // *** 04/09/01 swsrchf
        // *** start
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
        {
          local.InterstateCase1.Flag = "Y";
          local.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;
        }

        // *** end
        // *** 04/09/01 swsrchf
        // *** Problem report I00117417
        if (import.InterstateRequest.OtherStateFips != 0)
        {
          if (entities.InterstateRequest.OtherStateFips == import
            .InterstateRequest.OtherStateFips)
          {
            export.InterstateRequest.Assign(entities.InterstateRequest);
          }
        }
        else
        {
          export.InterstateRequest.Assign(entities.InterstateRequest);
        }

        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O')
        {
          export.InterstateRequest.OtherStateCaseClosureDate =
            local.Refresh.Date;
        }

        // *** Problem report I00121385
        // *** 06/12/01 swsrchf
        // *** added check for "CSI"
        local.Lo1.FunctionalTypeCode = "LO1";
        local.Csi.FunctionalTypeCode = "CSI";

        foreach(var item1 in ReadInterstateRequestHistory5())
        {
          ++local.InterstateRequestHistory.Count;
        }
      }
    }

    // ---------------------------------------------
    // If the Case contains multiple AP's, flow to
    // the Case_Composition screen.
    // ---------------------------------------------
    if (IsEmpty(import.Ap.Number))
    {
      foreach(var item in ReadCsePersonAbsentParent())
      {
        export.Ap.Number = entities.Ap.Number;
        ++local.Common.Count;

        if (local.Common.Count > 1)
        {
          if (AsChar(export.RetcompInd.Flag) == 'N')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
        }
      }
    }

    if (AsChar(entities.Case1.Status) == 'C')
    {
      if (ReadApplicantRecipientCsePerson2())
      {
        if (IsEmpty(export.Ar.FormattedName))
        {
          export.Ar.Number = entities.Ar.Number;
          UseSiReadCsePerson1();
        }
      }

      // >>
      // 11/01/2002 T.Bobb PR00138045
      // Futher qualified read by checking end date equal to case
      // status date. This corrected the problem when there were
      // two AR's that were end dated. The read used to get the
      // wrong one that wasn't associated to the interstate request.
      if (ReadAbsentParentCsePerson2())
      {
        if (IsEmpty(export.Ap.FormattedName))
        {
          export.Ap.Number = entities.Ap.Number;
          UseSiReadCsePerson2();
        }
      }
      else
      {
        ExitState = "NO_APS_ON_A_CASE";

        return;
      }
    }
    else
    {
      if (ReadApplicantRecipientCsePerson1())
      {
        export.Ar.Number = entities.Ar.Number;
        UseSiReadCsePerson1();
      }

      if (ReadAbsentParentCsePerson1())
      {
        export.Ap.Number = entities.Ap.Number;
        UseSiReadCsePerson2();
      }
      else
      {
        ExitState = "NO_APS_ON_A_CASE";

        return;
      }
    }

    local.InterstateCase1.Flag = "N";
    local.Lo1.FunctionalTypeCode = "LO1";
    local.Csi.FunctionalTypeCode = "CSI";

    if (!IsEmpty(export.OtherState.StateAbbreviation))
    {
      local.IvdAgency.CodeName = local.State.CodeName;
      local.CodeValue.Cdvalue = export.OtherState.StateAbbreviation;
    }
    else if (!IsEmpty(export.InterstateRequest.Country))
    {
      local.IvdAgency.CodeName = local.Country.CodeName;
      local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces(10);
    }
    else if (!IsEmpty(export.InterstateRequest.TribalAgency))
    {
      local.IvdAgency.CodeName = local.TribalAgency.CodeName;
      local.CodeValue.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
        (10);
    }

    UseCabGetCodeValueDescription();

    // *** Problem report I00121385
    // *** 06/12/01 swsrchf
    // *** added check for "CSI"
    // ******************** WR - 010501 ****************************
    // If request is not an incoming, check history to see if there is
    // an incoming that is closed.
    // Tom Bobb (swdptmb) 6/20/01
    // ******************************************************************
    if (ReadInterstateRequest2())
    {
      export.InterstateRequest.Assign(entities.InterstateRequest);

      // --------------------------------------------------------------------------------------
      // This is an outgoing request. Check for previous IIMC
      // interstate Request
      // ---------------------------------------------------------------------------------------
      if (ReadInterstateRequestHistory3())
      {
        ExitState = "ACO_NE0000_PREV_IN_INTRSTAT_CSE";
      }
      else
      {
        ExitState = "CASE_NOT_IC_INTERSTATE";
      }

      export.InterstateRequest.Assign(entities.InterstateRequest);
      export.InterstateRequest.CaseType = "";
      export.InterstateRequest.OtherStateCaseId = "";

      return;
    }

    if (IsEmpty(import.OtherState.StateAbbreviation) && IsEmpty
      (import.InterstateRequest.Country) && IsEmpty
      (import.InterstateRequest.TribalAgency))
    {
      local.MultipleIrForAp.Flag = "";
      local.InterstateRequestCount.Count = 0;

      // *** Problem report I00117417
      // *** 04/09/01 swsrchf
      // *** start
      local.InterstateCase1.Flag = "N";

      // *** end
      // *** 04/09/01 swsrchf
      // *** Problem report I00117417
      // *** Problem report I00121385
      // *** 06/12/01 swsrchf
      // *** start
      local.Lo1.FunctionalTypeCode = "LO1";
      local.Csi.FunctionalTypeCode = "CSI";
      local.FirstPass.Flag = "Y";

      // >>
      // 01/16/02 T.BOBB PR00127141 Set local previous state to 99 for Foreign 
      // Cases
      // in order to flow to IREQ if there is one domestic and one foreign for 
      // the
      // same case. Used to only pick up the domestic case.
      local.Previous.OtherStateFips = 99;
      local.N.KsCaseInd = "N";

      // *** end
      // *** 06/12/01 swsrchf
      // *** Problem report I00121385
      // *** Problem report I00117417
      // *** 04/09/01 swsrchf
      // *** Added Interstate_Request_History to the READ EACH
      // *** Problem report I00121385
      // *** 06/12/01 swsrchf
      // *** added check for "CSI"
      foreach(var item in ReadInterstateRequest6())
      {
        ++local.InterstateRequestCount.Count;
        local.InterstateRequest.IntHGeneratedId =
          entities.InterstateRequest.IntHGeneratedId;

        // *** Problem report I00121385
        // *** 06/12/01 swsrchf
        // *** start
        if (entities.InterstateRequest.OtherStateFips != local
          .Previous.OtherStateFips || !
          Equal(entities.InterstateRequest.Country, local.Previous.Country) || !
          Equal(entities.InterstateRequest.TribalAgency,
          local.Previous.TribalAgency))
        {
          if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O')
          {
            ++local.Open.Count;
          }
          else if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'C'
            )
          {
            ++local.Closed.Count;
          }

          switch(AsChar(local.FirstPass.Flag))
          {
            case 'N':
              if (local.Closed.Count > 1)
              {
                ExitState = "SI0000_MULTIPLE_IR_FOR_CLOSED_CA";

                return;
              }
              else if (local.Open.Count > 1)
              {
                ExitState = "SI0000_MULTIPLE_IR_EXISTS_FOR_AP";

                return;
              }

              // >>
              // If there is one open interstate request and one
              //  closed interstate request, display the open one.
              if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O'
                )
              {
                export.OtherState.State =
                  entities.InterstateRequest.OtherStateFips;
                export.InterstateRequest.Country =
                  entities.InterstateRequest.Country;
                export.InterstateRequest.TribalAgency =
                  entities.InterstateRequest.TribalAgency;
                local.InterstateRequest.IntHGeneratedId =
                  entities.InterstateRequest.IntHGeneratedId;
              }

              break;
            case 'Y':
              local.FirstPass.Flag = "N";
              MoveInterstateRequest2(entities.InterstateRequest, local.Previous);
                
              export.OtherState.State =
                entities.InterstateRequest.OtherStateFips;
              export.InterstateRequest.Country =
                entities.InterstateRequest.Country;
              export.InterstateRequest.TribalAgency =
                entities.InterstateRequest.TribalAgency;
              local.InterstateRequest.IntHGeneratedId =
                entities.InterstateRequest.IntHGeneratedId;

              break;
            default:
              break;
          }
        }

        // *** Problem report I00117417
        // *** 04/09/01 swsrchf
        // *** start
        local.InterstateCase1.Flag = "Y";

        // *** end
        // *** 04/09/01 swsrchf
        // *** Problem report I00117417
        // *** end
        // *** 06/12/01 swsrchf
        // *** Problem report I00121385
      }

      if (local.InterstateRequestCount.Count == 0)
      {
        // *** Problem report I00121385
        // *** 06/12/01 swsrchf
        // *** start
        local.Lo1.FunctionalTypeCode = "LO1";
        local.Csi.FunctionalTypeCode = "CSI";
        local.FirstPass.Flag = "Y";
        local.Previous.OtherStateFips = 99;
        local.N.KsCaseInd = "N";

        // *** end
        // *** 06/12/01 swsrchf
        // *** Problem report I00121385
        // *** Problem report I00117417
        // *** 04/09/01 swsrchf
        // *** Added Interstate_Request_History to the READ EACH
        // *** Problem report I00121385
        // *** 06/12/01 swsrchf
        // *** added check for "CSI"
        foreach(var item in ReadInterstateRequestCsePerson())
        {
          ++local.InterstateRequestCount.Count;
          local.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;

          // *** Problem report I00121385
          // *** 06/12/01 swsrchf
          // *** start
          if (entities.InterstateRequest.OtherStateFips != local
            .Previous.OtherStateFips || !
            Equal(entities.InterstateRequest.Country, local.Previous.Country) ||
            !
            Equal(entities.InterstateRequest.TribalAgency,
            local.Previous.TribalAgency))
          {
            if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O')
            {
              ++local.Open.Count;
            }
            else if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) ==
              'C')
            {
              ++local.Closed.Count;
            }

            switch(AsChar(local.FirstPass.Flag))
            {
              case 'N':
                if (local.Closed.Count > 1)
                {
                  ExitState = "SI0000_MULTIPLE_IR_FOR_CLOSED_CA";

                  return;
                }
                else if (local.Open.Count > 1)
                {
                  ExitState = "SI0000_MULTIPLE_IR_EXISTS_FOR_AP";

                  return;
                }

                // >>
                // If there is one open interstate request and one
                //  closed interstate request, display the open one.
                if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O'
                  )
                {
                  export.OtherState.State =
                    entities.InterstateRequest.OtherStateFips;
                  export.InterstateRequest.Country =
                    entities.InterstateRequest.Country;
                  export.InterstateRequest.TribalAgency =
                    entities.InterstateRequest.TribalAgency;
                  local.InterstateRequest.IntHGeneratedId =
                    entities.InterstateRequest.IntHGeneratedId;
                }

                break;
              case 'Y':
                local.FirstPass.Flag = "N";
                MoveInterstateRequest2(entities.InterstateRequest,
                  local.Previous);
                export.OtherState.State =
                  entities.InterstateRequest.OtherStateFips;
                export.InterstateRequest.Country =
                  entities.InterstateRequest.Country;
                export.InterstateRequest.TribalAgency =
                  entities.InterstateRequest.TribalAgency;
                local.InterstateRequest.IntHGeneratedId =
                  entities.InterstateRequest.IntHGeneratedId;

                break;
              default:
                break;
            }
          }

          // *** Problem report I00117417
          // *** 04/09/01 swsrchf
          // *** start
          local.InterstateCase1.Flag = "Y";

          // *** end
          // *** 04/09/01 swsrchf
          // *** Problem report I00117417
          // *** end
          // *** 06/12/01 swsrchf
          // *** Problem report I00121385
        }
      }

      // ******************** WR - 010501 ****************************
      // If request is not an incoming, check history to see if there is
      // an incoming that is closed.
      // Tom Bobb (swdptmb) 6/20/01
      // ******************************************************************
      if (local.InterstateRequestCount.Count == 0 || AsChar
        (local.InterstateCase1.Flag) != 'Y')
      {
        if (ReadInterstateRequest1())
        {
          export.InterstateRequest.Assign(entities.InterstateRequest);

          if (ReadInterstateRequestHistory1())
          {
            ExitState = "ACO_NE0000_PREV_IN_INTRSTAT_CSE";
          }
          else
          {
            ExitState = "CASE_NOT_IC_INTERSTATE";
          }
        }
        else
        {
          ExitState = "CASE_NOT_INTERSTATE";
        }

        return;
      }

      if (AsChar(local.MultipleIrForAp.Flag) != 'Y')
      {
        if (ReadInterstateRequest4())
        {
          // *** Problem report I00117417
          // *** 04/09/01 swsrchf
          // *** start
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y' || IsEmpty
            (entities.InterstateRequest.KsCaseInd))
          {
            // ******************** WR - 010501 ****************************
            // If request is not an incoming, check history to see if there is
            // an incoming that is closed.
            // Tom Bobb (swdptmb) 6/20/01
            // ******************************************************************
            if (ReadInterstateRequestHistory4())
            {
              ExitState = "ACO_NE0000_PREV_IN_INTRSTAT_CSE";
            }
            else
            {
              ExitState = "CASE_NOT_IC_INTERSTATE";
            }

            return;
          }

          // *** end
          // *** 04/09/01 swsrchf
          // *** Problem report I00117417
          local.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;
          export.InterstateRequest.Assign(entities.InterstateRequest);
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (ReadInterstateRequestAbsentParent2())
        {
          export.InterstateRequest.Assign(entities.InterstateRequest);

          // >>
          // 11/18/2002 T.Bobb PR00123201
          // Set the int_h_generated _id so in the read down below
          // can find the interstate request when it is a foreign country.
          local.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          local.InterstateRequestCount.Count = 0;

          foreach(var item in ReadInterstateRequest7())
          {
            ++local.InterstateRequestCount.Count;
            local.InterstateRequest.IntHGeneratedId =
              entities.InterstateRequest.IntHGeneratedId;
          }

          if (AsChar(export.Case1.Status) == 'O')
          {
            if (local.InterstateRequestCount.Count == 0)
            {
              ExitState = "CASE_NOT_INTERSTATE";
            }
            else
            {
              ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";
            }

            return;
          }
        }
      }

      if (ReadInterstateRequest5())
      {
        export.InterstateRequest.Assign(entities.InterstateRequest);
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
      else
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }

      if (entities.InterstateRequest.OtherStateFips > 0)
      {
        if (ReadFips2())
        {
          export.OtherState.Assign(entities.State);
        }
        else
        {
          ExitState = "FIPS_NF";

          return;
        }
      }
    }
    else
    {
      if (!IsEmpty(import.OtherState.StateAbbreviation))
      {
        if (ReadFips3())
        {
          export.OtherState.Assign(entities.State);
        }
        else
        {
          ExitState = "FIPS_NF";

          return;
        }
      }

      if (ReadInterstateRequest3())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y' || IsEmpty
          (entities.InterstateRequest.KsCaseInd))
        {
          export.InterstateRequest.Assign(entities.InterstateRequest);
          ExitState = "CASE_NOT_IC_INTERSTATE";

          return;
        }

        export.InterstateRequest.Assign(entities.InterstateRequest);
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
      else if (ReadInterstateRequestAbsentParent1())
      {
        export.InterstateRequest.Assign(entities.InterstateRequest);
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
      else
      {
        local.InterstateRequestCount.Count = 0;

        foreach(var item in ReadInterstateRequest7())
        {
          ++local.InterstateRequestCount.Count;
          local.InterstateRequest.IntHGeneratedId =
            entities.InterstateRequest.IntHGeneratedId;
        }

        if (AsChar(export.Case1.Status) == 'O')
        {
          if (local.InterstateRequestCount.Count == 0)
          {
            ExitState = "CASE_NOT_INTERSTATE";
          }
          else if (!IsEmpty(import.OtherState.StateAbbreviation))
          {
            ExitState = "AP_NOT_INVOLVED_IN_INTERSTATE";
          }
          else if (!IsEmpty(import.InterstateRequest.Country))
          {
            ExitState = "AP_NOT_FOREIGN_INTERSTATE";
          }
          else if (!IsEmpty(import.InterstateRequest.TribalAgency))
          {
            ExitState = "AP_NOT_TRIBAL_INTERSTATE";
          }

          return;
        }
      }
    }

    if (!IsEmpty(export.OtherState.StateAbbreviation))
    {
      local.IvdAgency.CodeName = local.State.CodeName;
      local.CodeValue.Cdvalue = export.OtherState.StateAbbreviation;
    }
    else if (!IsEmpty(export.InterstateRequest.Country))
    {
      local.IvdAgency.CodeName = local.Country.CodeName;
      local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces(10);
    }
    else if (!IsEmpty(export.InterstateRequest.TribalAgency))
    {
      local.IvdAgency.CodeName = local.TribalAgency.CodeName;
      local.CodeValue.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
        (10);
    }

    UseCabGetCodeValueDescription();
    export.InterstateRequestCount.Count = local.InterstateRequestCount.Count + local
      .InterstateRequestHistory.Count;

    if (local.InterstateRequestCount.Count == 1)
    {
      if (ReadInterstateRequest5())
      {
        export.InterstateRequest.Assign(entities.InterstateRequest);
      }
      else
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }
    }

    // >>
    // Added this code until the PR to fix contact info can be finished.
    // Remove after fix is completed.
    // >>
    // Enable this code when PR to fix contact info is finished.
    if (ReadInterstateContact())
    {
      export.InterstateContact.Assign(entities.InterstateContact);
      local.Ct.Type1 = "CT";

      if (ReadInterstateContactAddress())
      {
        export.InterstateContactAddress.
          Assign(entities.InterstateContactAddress);
      }
    }

    if (ReadInterstatePaymentAddress())
    {
      export.InterstatePaymentAddress.Assign(entities.InterstatePaymentAddress);

      if (!IsEmpty(export.InterstatePaymentAddress.State))
      {
        if (ReadFips1())
        {
          export.InterstatePaymentAddress.FipsState =
            NumberToString(entities.Fips.State, 2);
          export.InterstatePaymentAddress.FipsCounty = "000";
          export.InterstatePaymentAddress.FipsLocation = "00";
        }
      }
    }

    local.Iicnv.ActionReasonCode = "IICNV";

    // >>
    // 2/8/2002 T.Bobb Get the latest manual conversion record
    ReadInterstateRequestHistory2();

    if (entities.InterstateRequestHistory.Populated)
    {
      MoveInterstateRequestHistory(entities.InterstateRequestHistory,
        export.Note);
    }

    export.IreqCreated.Date = Date(entities.InterstateRequest.CreatedTimestamp);
    export.IreqUpdated.Date =
      Date(entities.InterstateRequest.LastUpdatedTimestamp);

    if (ReadInterstateCase1())
    {
      export.InterstateCase.Assign(entities.InterstateCase);
    }
    else
    {
      local.InterstateCase2.TransSerialNumber = 0;

      if (entities.Case1.IcTransSerialNumber == 0)
      {
        if (Equal(entities.Case1.IcTransactionDate, null) || Equal
          (entities.Case1.IcTransactionDate, new DateTime(2099, 12, 31)))
        {
          foreach(var item in ReadInterstateCase2())
          {
            if (entities.InterstateCase.TransSerialNumber > local
              .InterstateCase2.TransSerialNumber)
            {
              local.InterstateCase2.TransSerialNumber =
                entities.InterstateCase.TransSerialNumber;
            }
          }

          ++local.InterstateCase2.TransSerialNumber;
        }
        else
        {
          foreach(var item in ReadInterstateCase3())
          {
            if (entities.InterstateCase.TransSerialNumber > local
              .InterstateCase2.TransSerialNumber)
            {
              local.InterstateCase2.TransSerialNumber =
                entities.InterstateCase.TransSerialNumber;
            }
          }

          ++local.InterstateCase2.TransSerialNumber;
        }
      }
      else
      {
        local.InterstateCase2.TransSerialNumber =
          entities.Case1.IcTransSerialNumber + 1;
      }

      try
      {
        CreateInterstateCase();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (IsEmpty(export.OtherState.StateAbbreviation))
    {
      export.OtherState.State = 0;
      export.DisplayOnly.DuplicateCaseIndicator = "";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.TransactionDate = source.TransactionDate;
    target.ActionReasonCode = source.ActionReasonCode;
    target.Note = source.Note;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.IvdAgency.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.State.CodeName = useExport.State.CodeName;
    local.TribalAgency.CodeName = useExport.TribalAgency.CodeName;
    local.Country.CodeName = useExport.Country.CodeName;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ap);
  }

  private void CreateInterstateCase()
  {
    var transSerialNumber = local.InterstateCase2.TransSerialNumber;
    var transactionDate = Now().Date;

    entities.InterstateCase.Populated = false;
    Update("CreateInterstateCase",
      (db, command) =>
      {
        db.SetInt32(command, "localFipsState", 0);
        db.SetNullableInt32(command, "localFipsCounty", 0);
        db.SetInt64(command, "transSerialNbr", transSerialNumber);
        db.SetString(command, "actionCode", "");
        db.SetString(command, "functionalTypeCo", "");
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "ksCaseId", "");
        db.SetNullableString(command, "actionReasonCode", "");
        db.SetNullableDate(command, "actionResolution", default(DateTime));
        db.SetNullableInt32(command, "caseDataInd", 0);
        db.SetNullableTimeSpan(command, "sentTime", TimeSpan.Zero);
        db.SetNullableString(command, "paymentMailingAd", "");
        db.SetNullableString(command, "paymentState", "");
        db.SetNullableString(command, "paymentZipCode4", "");
        db.SetNullableString(command, "contactNameLast", "");
        db.SetNullableString(command, "contactNameFirst", "");
        db.SetNullableInt32(command, "contactPhoneNum", 0);
        db.SetNullableString(command, "memo", "");
        db.SetNullableString(command, "contactPhoneExt", "");
        db.SetNullableString(command, "conInternetAddr", "");
        db.SetNullableString(command, "sendPaymBankAcc", "");
        db.SetNullableInt32(command, "sendPaymRtCode", 0);
      });

    entities.InterstateCase.TransSerialNumber = transSerialNumber;
    entities.InterstateCase.TransactionDate = transactionDate;
    entities.InterstateCase.SendPaymentsBankAccount = "";
    entities.InterstateCase.SendPaymentsRoutingCode = 0;
    entities.InterstateCase.Populated = true;
  }

  private bool ReadAbsentParentCsePerson1()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadAbsentParentCsePerson2()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", export.Case1.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadApplicantRecipientCsePerson1()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadApplicantRecipientCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadApplicantRecipientCsePerson2()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadApplicantRecipientCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", export.Case1.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.AbsentParent.CasNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation",
          export.InterstatePaymentAddress.State ?? "");
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

  private bool ReadFips2()
  {
    entities.State.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.OtherState.State);
      },
      (db, reader) =>
      {
        entities.State.State = db.GetInt32(reader, 0);
        entities.State.County = db.GetInt32(reader, 1);
        entities.State.Location = db.GetInt32(reader, 2);
        entities.State.StateAbbreviation = db.GetString(reader, 3);
        entities.State.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.State.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.OtherState.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.State.State = db.GetInt32(reader, 0);
        entities.State.County = db.GetInt32(reader, 1);
        entities.State.Location = db.GetInt32(reader, 2);
        entities.State.StateAbbreviation = db.GetString(reader, 3);
        entities.State.Populated = true;
      });
  }

  private bool ReadInterstateCase1()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase1",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", entities.Case1.IcTransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          entities.Case1.IcTransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 2);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 3);
        entities.InterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCase2()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase2",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          entities.Case1.CseOpenDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 2);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 3);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCase3()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase3",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          entities.Case1.IcTransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 2);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 3);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", export.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.ContactPhoneNum =
          db.GetNullableInt32(reader, 2);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 3);
        entities.InterstateContact.CreatedBy = db.GetString(reader, 4);
        entities.InterstateContact.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 6);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 7);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 8);
        entities.InterstateContact.ContactNameSuffix =
          db.GetNullableString(reader, 9);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 12);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 14);
        entities.InterstateContact.Populated = true;
      });
  }

  private bool ReadInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);
    entities.InterstateContactAddress.Populated = false;

    return Read("ReadInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.CreatedBy = db.GetString(reader, 3);
        entities.InterstateContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateContactAddress.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.InterstateContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 7);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateContactAddress.County =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 17);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 18);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 19);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 20);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 21);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 22);
        entities.InterstateContactAddress.Populated = true;
        CheckValid<InterstateContactAddress>("LocationType",
          entities.InterstateContactAddress.LocationType);
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", export.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 5);
        entities.InterstatePaymentAddress.Zip5 =
          db.GetNullableString(reader, 6);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstatePaymentAddress.CreatedBy = db.GetString(reader, 8);
        entities.InterstatePaymentAddress.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.InterstatePaymentAddress.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.InterstatePaymentAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 12);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.InterstatePaymentAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.InterstatePaymentAddress.County =
          db.GetNullableString(reader, 17);
        entities.InterstatePaymentAddress.Street3 =
          db.GetNullableString(reader, 18);
        entities.InterstatePaymentAddress.Street4 =
          db.GetNullableString(reader, 19);
        entities.InterstatePaymentAddress.Province =
          db.GetNullableString(reader, 20);
        entities.InterstatePaymentAddress.PostalCode =
          db.GetNullableString(reader, 21);
        entities.InterstatePaymentAddress.Country =
          db.GetNullableString(reader, 22);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 23);
        entities.InterstatePaymentAddress.FipsCounty =
          db.GetNullableString(reader, 24);
        entities.InterstatePaymentAddress.FipsState =
          db.GetNullableString(reader, 25);
        entities.InterstatePaymentAddress.FipsLocation =
          db.GetNullableString(reader, 26);
        entities.InterstatePaymentAddress.RoutingNumberAba =
          db.GetNullableInt64(reader, 27);
        entities.InterstatePaymentAddress.AccountNumberDfi =
          db.GetNullableString(reader, 28);
        entities.InterstatePaymentAddress.AccountType =
          db.GetNullableString(reader, 29);
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetInt32(
          command, "otherStateFips", import.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "country", import.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", import.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest3()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(command, "othrStateFipsCd", export.OtherState.State);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest4()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest4",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(command, "state", export.OtherState.State);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest5()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest5",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", local.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest6()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest6",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetString(command, "ksCaseInd", local.N.KsCaseInd);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest7()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest7",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestAbsentParent1()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetInt32(command, "othrStateFipsCd", export.OtherState.State);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParent2()
  {
    entities.InterstateRequest.Populated = false;
    entities.AbsentParent.Populated = false;

    return Read("ReadInterstateRequestAbsentParent2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetInt32(command, "state", export.OtherState.State);
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.AbsentParent.CasNumber = db.GetString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.AbsentParent.CspNumber = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.AbsentParent.Type1 = db.GetString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 19);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 20);
        entities.InterstateRequest.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCsePerson()
  {
    entities.InterstateRequest.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadInterstateRequestCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
        db.SetString(command, "ksCaseInd", local.N.KsCaseInd);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.Ap.Number = db.GetString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestHistory1()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistory2()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", export.InterstateRequest.IntHGeneratedId);
        db.SetNullableString(
          command, "actionReasonCode", local.Iicnv.ActionReasonCode ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistory3()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory3",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.
          SetString(command, "functionalTypeCo1", local.Lo1.FunctionalTypeCode);
          
        db.
          SetString(command, "functionalTypeCo2", local.Csi.FunctionalTypeCode);
          
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistory4()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory4",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory5()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory5",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.
          SetString(command, "functionalTypeCo1", local.Lo1.FunctionalTypeCode);
          
        db.
          SetString(command, "functionalTypeCo2", local.Csi.FunctionalTypeCode);
          
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;

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
    /// A value of RetcompInd.
    /// </summary>
    [JsonPropertyName("retcompInd")]
    public Common RetcompInd
    {
      get => retcompInd ??= new();
      set => retcompInd = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Common retcompInd;
    private CsePersonsWorkSet ar;
    private InterstateRequest interstateRequest;
    private Fips otherState;
    private Case1 case1;
    private CsePersonsWorkSet ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateRequestCount.
    /// </summary>
    [JsonPropertyName("interstateRequestCount")]
    public Common InterstateRequestCount
    {
      get => interstateRequestCount ??= new();
      set => interstateRequestCount = value;
    }

    /// <summary>
    /// A value of RetcompInd.
    /// </summary>
    [JsonPropertyName("retcompInd")]
    public Common RetcompInd
    {
      get => retcompInd ??= new();
      set => retcompInd = value;
    }

    /// <summary>
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of IreqUpdated.
    /// </summary>
    [JsonPropertyName("ireqUpdated")]
    public DateWorkArea IreqUpdated
    {
      get => ireqUpdated ??= new();
      set => ireqUpdated = value;
    }

    /// <summary>
    /// A value of IreqCreated.
    /// </summary>
    [JsonPropertyName("ireqCreated")]
    public DateWorkArea IreqCreated
    {
      get => ireqCreated ??= new();
      set => ireqCreated = value;
    }

    /// <summary>
    /// A value of Note.
    /// </summary>
    [JsonPropertyName("note")]
    public InterstateRequestHistory Note
    {
      get => note ??= new();
      set => note = value;
    }

    /// <summary>
    /// A value of ContactPhone.
    /// </summary>
    [JsonPropertyName("contactPhone")]
    public InterstateWorkArea ContactPhone
    {
      get => contactPhone ??= new();
      set => contactPhone = value;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
    }

    private Common interstateRequestCount;
    private Common retcompInd;
    private CodeValue agency;
    private DateWorkArea ireqUpdated;
    private DateWorkArea ireqCreated;
    private InterstateRequestHistory note;
    private InterstateWorkArea contactPhone;
    private Case1 case1;
    private Fips otherState;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateCase interstateCase;
    private Case1 displayOnly;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IvdAgency.
    /// </summary>
    [JsonPropertyName("ivdAgency")]
    public Code IvdAgency
    {
      get => ivdAgency ??= new();
      set => ivdAgency = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of TribalAgency.
    /// </summary>
    [JsonPropertyName("tribalAgency")]
    public Code TribalAgency
    {
      get => tribalAgency ??= new();
      set => tribalAgency = value;
    }

    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Common Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Common Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of FirstPass.
    /// </summary>
    [JsonPropertyName("firstPass")]
    public Common FirstPass
    {
      get => firstPass ??= new();
      set => firstPass = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public InterstateRequest Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Ct.
    /// </summary>
    [JsonPropertyName("ct")]
    public InterstateContactAddress Ct
    {
      get => ct ??= new();
      set => ct = value;
    }

    /// <summary>
    /// A value of N.
    /// </summary>
    [JsonPropertyName("n")]
    public InterstateRequest N
    {
      get => n ??= new();
      set => n = value;
    }

    /// <summary>
    /// A value of Csi.
    /// </summary>
    [JsonPropertyName("csi")]
    public InterstateRequestHistory Csi
    {
      get => csi ??= new();
      set => csi = value;
    }

    /// <summary>
    /// A value of Lo1.
    /// </summary>
    [JsonPropertyName("lo1")]
    public InterstateRequestHistory Lo1
    {
      get => lo1 ??= new();
      set => lo1 = value;
    }

    /// <summary>
    /// A value of Iicnv.
    /// </summary>
    [JsonPropertyName("iicnv")]
    public InterstateRequestHistory Iicnv
    {
      get => iicnv ??= new();
      set => iicnv = value;
    }

    /// <summary>
    /// A value of Refresh.
    /// </summary>
    [JsonPropertyName("refresh")]
    public DateWorkArea Refresh
    {
      get => refresh ??= new();
      set => refresh = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public Common InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequestCount.
    /// </summary>
    [JsonPropertyName("interstateRequestCount")]
    public Common InterstateRequestCount
    {
      get => interstateRequestCount ??= new();
      set => interstateRequestCount = value;
    }

    /// <summary>
    /// A value of InterstateCase1.
    /// </summary>
    [JsonPropertyName("interstateCase1")]
    public Common InterstateCase1
    {
      get => interstateCase1 ??= new();
      set => interstateCase1 = value;
    }

    /// <summary>
    /// A value of InterstateCase2.
    /// </summary>
    [JsonPropertyName("interstateCase2")]
    public InterstateCase InterstateCase2
    {
      get => interstateCase2 ??= new();
      set => interstateCase2 = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of MultipleIrForAp.
    /// </summary>
    [JsonPropertyName("multipleIrForAp")]
    public Common MultipleIrForAp
    {
      get => multipleIrForAp ??= new();
      set => multipleIrForAp = value;
    }

    /// <summary>
    /// A value of OutgoingInterstate.
    /// </summary>
    [JsonPropertyName("outgoingInterstate")]
    public Common OutgoingInterstate
    {
      get => outgoingInterstate ??= new();
      set => outgoingInterstate = value;
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

    private Code ivdAgency;
    private Code state;
    private Code tribalAgency;
    private Common closed;
    private Common open;
    private Common firstPass;
    private InterstateRequest previous;
    private InterstateContactAddress ct;
    private InterstateRequest n;
    private InterstateRequestHistory csi;
    private InterstateRequestHistory lo1;
    private InterstateRequestHistory iicnv;
    private DateWorkArea refresh;
    private Common interstateRequestHistory;
    private Common interstateRequestCount;
    private Common interstateCase1;
    private InterstateCase interstateCase2;
    private CodeValue codeValue;
    private InterstateRequest interstateRequest;
    private DateWorkArea current;
    private Code country;
    private Common error;
    private Common common;
    private Common multipleIrForAp;
    private Common outgoingInterstate;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Fips State
    {
      get => state ??= new();
      set => state = value;
    }

    private InterstateCase interstateCase;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private Fips fips;
    private InterstateRequest interstateRequest;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson ap;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private Fips state;
  }
#endregion
}
