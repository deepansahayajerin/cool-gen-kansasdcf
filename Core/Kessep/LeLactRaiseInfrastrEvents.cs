// Program: LE_LACT_RAISE_INFRASTR_EVENTS, ID: 371985174, model: 746.
// Short name: SWE01841
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
/// A program: LE_LACT_RAISE_INFRASTR_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the event(s) for Infrastructure.
/// It is used in the PRAD LE LACT LEGAL ACTION except when the legal action is 
/// the first for the case.  In this situation, due to monitored activies, it is
/// used in LE LROL LEGAL ROLE.
/// </para>
/// </summary>
[Serializable]
public partial class LeLactRaiseInfrastrEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LACT_RAISE_INFRASTR_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLactRaiseInfrastrEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLactRaiseInfrastrEvents.
  /// </summary>
  public LeLactRaiseInfrastrEvents(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E     L O G
    // ----------------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ---------	------------	
    // ------------------------------------------------------------
    // 11/12/96  govind			Initial code.
    // 12/31/96  govind			Modified Code based on Serv Plan Doc dated 12-20-1996.
    // 01/02/97  govind			Added code for External alert
    // 01/02/97  govind	107		Update Paternity Ind in Child Case Role
    // 09/08/97  Alan Samuels	PR 27171
    // 09/09/97  Alan Samuels	PR 27335
    // 09/30/97  R Grey			Add new Events, IDCR #
    // 10/08/97  R Grey	H00026837	Add external alerts
    // 11/14/97  R Grey			Add new events; sync class with act taken
    // 03/06/99  PMcElderry			Logic for monitored activities:
    // 					Event 95 (Details 290-408); Event 30 (Details 511, 512);
    // 					Event 31 (Detail 527); Event 32 (Detail 559);
    // 					Event 33 (Details 573, 574); Event 38 (Details 610-613);
    // 					Event 97 (Details 100-119); Event 96 (Details 200, 201);
    // 					Event 98 (Details 306, 308, 310),
    // 					Event 36 (Details 223, 362, 600);
    // 					Event 99 (Details 100, 200)
    // 04/07/99  PMcElderry			Changed GARNREQ to GARNRQW; added GARNRQNW.
    // 02/06/99  Anand		PR# H00079233	Process Only Open Cases
    // 04/04/00  DJean		WO# 160R	Change to set new Establish Paternity 
    // Indicators
    // 05/23/00  J.Magat	PR# 95740	Extend setting of Paternity Indicator to 
    // Inactive CH in
    // 					active cases.  Added call to SI_CAB_VALIDATE_PATERNITY_COMB.
    // 05/26/00  J.Magat	PR# 78408	Receiving error message - SP0000: Event 
    // Detail not found
    // 07/06/00  J.Magat	PR# 98481	Prior to the establishment of child 
    // paternity, check cases
    // 					related to the legal action details for any male APs.
    // 					If no male APs, skip paternity establishment logic.
    // 08/31/00  GVandy	PR# 99649	Raise event 95 for U class legal actions 
    // AFFEP, CONTORDN,
    // 					LOCDATA, REGSTATE, TESTIMON, TRANS1, TRANS2, TRANS3 and UNIFORMP.
    // 04/10/03  GVandy	PR 160591	Raise Event 95 for ALL legal actions.
    // 07/21/10  JHuss		CQ# 358		Add VOLGENOR to Event 33 list
    // 09/28/16  AHockman       cq41786        Add ORDIWOLS and NOTIWOLS to 
    // event 38 list
    // 05/10/17  GVandy	CQ48108		IV-D PEP changes.
    // ----------------------------------------------------------------------------------------------------
    local.Zero.Date = null;
    local.Current.Date = Now().Date;
    local.HearingDtPriorToFiling.Flag = "N";

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF_RB";

      return;
    }

    local.Infrastructure.SituationNumber = 0;

    // ----------------------------------------------------------------
    // Following logic will only be executed after a new legal
    // action has been added to the system -  used in LACT and
    // LROL
    // ----------------------------------------------------------------
    if (!IsEmpty(import.Event95ForNewLegActn.Flag))
    {
      // ----------------------------------------------------
      // Legal Action has not yet been filed - raise event 95
      // ----------------------------------------------------
      local.Code.CodeName = "ACTION TAKEN";
      local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
      UseCabGetCodeValueDescription();
      local.Infrastructure.ReasonCode = "A" + entities.LegalAction.ActionTaken;
      UseLeCabGetRelCaseUnitsFLact();
      local.Infrastructure.EventId = 95;
      local.Infrastructure.CsenetInOutCode = "";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.UserId = "LACT";
      local.Infrastructure.BusinessObjectCd = "LEA";
      local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
      local.Infrastructure.DenormText12 = entities.LegalAction.CourtCaseNumber;
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.Detail = local.CodeValue.Description;
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.CreatedTimestamp = Now();

      if (!IsEmpty(entities.LegalAction.InitiatingState) && !
        Equal(entities.LegalAction.InitiatingState, "KS"))
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
        .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
      {
        if (!local.RelatedCaseUnits.CheckSize())
        {
          break;
        }

        local.Infrastructure.CaseNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number;
        local.Infrastructure.CaseUnitNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCuCaseUnit.CuNumber;
        local.Infrastructure.CsePersonNumber =
          local.RelatedCaseUnits.Item.DtlRelatedObligor.Number;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          // -------------------
          // Continue processing
          // -------------------
        }
      }

      local.RelatedCaseUnits.CheckIndex();

      // ---------------------------------------------------------
      // This is the only event during the add of a legal action
      // ---------------------------------------------------------
      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    // -----------------
    // All common code
    // -----------------
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "LACT";
    local.Infrastructure.BusinessObjectCd = "LEA";
    local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
    local.Infrastructure.DenormText12 = entities.LegalAction.CourtCaseNumber;
    local.Infrastructure.ReferenceDate = entities.LegalAction.FiledDate;
    local.Infrastructure.Detail = local.CodeValue.Description;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();

    if (!IsEmpty(entities.LegalAction.InitiatingState) && !
      Equal(entities.LegalAction.InitiatingState, "KS"))
    {
      local.Infrastructure.InitiatingStateCode = "OS";
    }
    else
    {
      local.Infrastructure.InitiatingStateCode = "KS";
    }

    local.Code.CodeName = "ACTION TAKEN";
    local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
    UseCabGetCodeValueDescription();

    // -------------------------------------------------------------
    // Default to ..X. It will be set to a proper value later below.
    // -------------------------------------------------------------
    switch(AsChar(entities.LegalAction.Classification))
    {
      case 'P':
        local.Infrastructure.ReasonCode = "FPETITIONX";
        local.Infrastructure.EventId = 33;

        break;
      case 'M':
        local.Infrastructure.ReasonCode = "FMOTIONX";
        local.Infrastructure.EventId = 31;

        break;
      case 'O':
        local.Infrastructure.ReasonCode = "FORDERX";
        local.Infrastructure.EventId = 32;

        break;
      case 'J':
        local.Infrastructure.ReasonCode = "FJEX";
        local.Infrastructure.EventId = 30;

        break;
      default:
        break;
    }

    // ---------------------------------------------
    // Event 28 E LE ACTION JOURNAL ENTRY
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "718BJERJ") || Equal
      (entities.LegalAction.ActionTaken, "718BDEFJ") || Equal
      (entities.LegalAction.ActionTaken, "DFLTSUPJ") || Equal
      (entities.LegalAction.ActionTaken, "SUPPORTJ") || Equal
      (entities.LegalAction.ActionTaken, "DEFJPATJ") || Equal
      (entities.LegalAction.ActionTaken, "PATERNJ") || Equal
      (entities.LegalAction.ActionTaken, "JEF") || Equal
      (entities.LegalAction.ActionTaken, "QUALMEDO") || Equal
      (entities.LegalAction.ActionTaken, "VOLPATTJ") || Equal
      (entities.LegalAction.ActionTaken, "VOLSUPTJ") || Equal
      (entities.LegalAction.ActionTaken, "MODSUPPO") || Equal
      (entities.LegalAction.ActionTaken, "SETARRSJ") || Equal
      (entities.LegalAction.ActionTaken, "MEDEXPJ") || Equal
      (entities.LegalAction.ActionTaken, "JEFBC") || Equal
      (entities.LegalAction.ActionTaken, "JEFMODO") || Equal
      (entities.LegalAction.ActionTaken, "MODBC") || Equal
      (entities.LegalAction.ActionTaken, "JENF") || Equal
      (entities.LegalAction.ActionTaken, "ORDER") || Equal
      (entities.LegalAction.ActionTaken, "ORDERKS"))
    {
      // ----------------------------------------------------
      // For event 28, there is no prefix "F"
      // ----------------------------------------------------
      foreach(var item in ReadLegalActionDetail2())
      {
        switch(TrimEnd(entities.LegalActionDetail.NonFinOblgType))
        {
          case "EP":
            local.Infrastructure.ReasonCode = "LEDTLEPEST";

            break;
          case "HIC":
            local.Infrastructure.ReasonCode = "LEDTLHICEST";

            break;
          default:
            if (ReadObligationType())
            {
              switch(TrimEnd(entities.ObligationType.Code))
              {
                case "BDCK RC":
                  local.Infrastructure.ReasonCode = "LEDTLBDCKRCEST";

                  break;
                case "IRS NEG":
                  local.Infrastructure.ReasonCode = "LEDTLIRSNEGEST";

                  break;
                case "MIS AR":
                  local.Infrastructure.ReasonCode = "LEDTLMISAREST";

                  break;
                case "MIS AP":
                  local.Infrastructure.ReasonCode = "LEDTLMISAPEST";

                  break;
                case "718B":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTL718BESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTL718BEST";
                  }

                  break;
                case "AJ":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLAJESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLAJEST";
                  }

                  break;
                case "CS":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLCSESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLCSEST";
                  }

                  break;
                case "MC":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLMCESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLMCEST";
                  }

                  break;
                case "MJ":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLMJESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLMJEST";
                  }

                  break;
                case "MS":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLMSESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLMSEST";
                  }

                  break;
                case "SP":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLSPESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLSPEST";
                  }

                  break;
                case "SAJ":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLSAJESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLSAJEST";
                  }

                  break;
                case "CRCH":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLCRCHESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLCRCHEST";
                  }

                  break;
                case "IJ":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLIJESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLIJEST";
                  }

                  break;
                case "VOL":
                  local.Infrastructure.ReasonCode = "LEDTLVOLEST";

                  break;
                case "FEE":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLFEEESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLFEEEST";
                  }

                  break;
                case "IVD RC":
                  if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
                    (entities.LegalAction.EstablishmentCode, "CT"))
                  {
                    local.Infrastructure.ReasonCode = "LEDTLIVDRCESTKS";
                  }
                  else
                  {
                    local.Infrastructure.ReasonCode = "LEDTLIVDRCEST";
                  }

                  break;
                default:
                  continue;
              }
            }
            else
            {
              continue;
            }

            break;
        }

        // *** Event-id 28 will be generated only for existing legal
        // details with above identified financial obligations and for
        // non-financial obligations EP and HIC.
        local.Infrastructure.EventId = 28;
        UseLeCabGetRelCaseUnitsFLdet();

        // -------------------------------------
        // Create Infrastructure records
        // -------------------------------------
        for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
          .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
        {
          if (!local.RelatedCaseUnits.CheckSize())
          {
            break;
          }

          local.Infrastructure.CaseNumber =
            local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number;
          local.Infrastructure.CaseUnitNumber =
            local.RelatedCaseUnits.Item.DetailRelatedCuCaseUnit.CuNumber;
          local.Infrastructure.CsePersonNumber =
            local.RelatedCaseUnits.Item.DtlRelatedObligor.Number;
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }

        local.RelatedCaseUnits.CheckIndex();

        // *** Reset processed Event-id.
        local.Infrastructure.EventId = 0;
      }
    }

    // ---------------------------------------------
    // Event 30 E LE ACTION JOURNAL ENTRY
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "718BJERJ") || Equal
      (entities.LegalAction.ActionTaken, "718BDEFJ") || Equal
      (entities.LegalAction.ActionTaken, "DFLTSUPJ") || Equal
      (entities.LegalAction.ActionTaken, "SUPPORTJ") || Equal
      (entities.LegalAction.ActionTaken, "MODSUPPO") || Equal
      (entities.LegalAction.ActionTaken, "DEFJPATJ") || Equal
      (entities.LegalAction.ActionTaken, "PATERNJ") || Equal
      (entities.LegalAction.ActionTaken, "JEF") || Equal
      (entities.LegalAction.ActionTaken, "SETARRSJ") || Equal
      (entities.LegalAction.ActionTaken, "718BJERB") || Equal
      (entities.LegalAction.ActionTaken, "VOLPATTJ") || Equal
      (entities.LegalAction.ActionTaken, "VOLSUPTJ") || Equal
      (entities.LegalAction.ActionTaken, "VOL718BJ") || Equal
      (entities.LegalAction.ActionTaken, "EMPIWOJ") || Equal
      (entities.LegalAction.ActionTaken, "MEDEXPJ") || Equal
      (entities.LegalAction.ActionTaken, "EMPMWOJ") || Equal
      (entities.LegalAction.ActionTaken, "REGENFO") || Equal
      (entities.LegalAction.ActionTaken, "REGMODO") || Equal
      (entities.LegalAction.ActionTaken, "PATONLYJ") || Equal
      (entities.LegalAction.ActionTaken, "JEX"))
    {
      local.Infrastructure.EventId = 30;

      if (Equal(entities.LegalAction.ActionTaken, "JEF"))
      {
        local.Infrastructure.ReasonCode = "FJEFJ";
      }
      else
      {
        local.Infrastructure.ReasonCode = "F" + entities
          .LegalAction.ActionTaken;
      }
    }

    // ---------------------------------------------
    // Event 31 E LE ACTION MOTION FILED
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "PRCSRVMO") || Equal
      (entities.LegalAction.ActionTaken, "DELTSUPM") || Equal
      (entities.LegalAction.ActionTaken, "DEFJPATM") || Equal
      (entities.LegalAction.ActionTaken, "718BDEFM") || Equal
      (entities.LegalAction.ActionTaken, "AIDOEXMO") || Equal
      (entities.LegalAction.ActionTaken, "CONTEMPT") || Equal
      (entities.LegalAction.ActionTaken, "CONTINUM") || Equal
      (entities.LegalAction.ActionTaken, "REVIVORM") || Equal
      (entities.LegalAction.ActionTaken, "GARDADLM") || Equal
      (entities.LegalAction.ActionTaken, "GALAMFM") || Equal
      (entities.LegalAction.ActionTaken, "COMPENIM") || Equal
      (entities.LegalAction.ActionTaken, "JDAGEMPM") || Equal
      (entities.LegalAction.ActionTaken, "JUDMEXPM") || Equal
      (entities.LegalAction.ActionTaken, "MEDESTBM") || Equal
      (entities.LegalAction.ActionTaken, "MODMSOM") || Equal
      (entities.LegalAction.ActionTaken, "CSMODM") || Equal
      (entities.LegalAction.ActionTaken, "NUNCPROM") || Equal
      (entities.LegalAction.ActionTaken, "GENETICM") || Equal
      (entities.LegalAction.ActionTaken, "DISMISSM") || Equal
      (entities.LegalAction.ActionTaken, "GENWRITM") || Equal
      (entities.LegalAction.ActionTaken, "CONSOLDM") || Equal
      (entities.LegalAction.ActionTaken, "SETARRSM") || Equal
      (entities.LegalAction.ActionTaken, "MOTOSTAY") || Equal
      (entities.LegalAction.ActionTaken, "RULE170N") || Equal
      (entities.LegalAction.ActionTaken, "ENROLLM") || Equal
      (entities.LegalAction.ActionTaken, "COMPENVM") || Equal
      (entities.LegalAction.ActionTaken, "MOTIONKS") || Equal
      (entities.LegalAction.ActionTaken, "PROTECTM") || Equal
      (entities.LegalAction.ActionTaken, "QUASHM"))
    {
      local.Infrastructure.EventId = 31;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;

      // ----------------------------------------------------------------
      // Determine if a hearing date has already been set for the
      // motion filed.  If so control raising event so as to close the
      // Monitored Activity created to monitor for setting a hearing
      // for a Motion.  RCG
      // -----------------------------------------------------------------
      foreach(var item in ReadHearing())
      {
        if (Equal(entities.Existing.OutcomeReceivedDate, local.Zero.Date))
        {
          local.HearingDtPriorToFiling.Flag = "Y";
          local.PriorHearing.Assign(entities.Existing);

          break;
        }
      }
    }
    else if (Equal(entities.LegalAction.ActionTaken, "LIMINEM"))
    {
      local.Infrastructure.EventId = 31;
      local.Infrastructure.ReasonCode = entities.LegalAction.ActionTaken;

      // ----------------------------------------------------------------
      // Determine if a hearing date has already been set for the
      // motion filed.  If so control raising event so as to close the
      // Monitored Activity created to monitor for setting a hearing
      // for a Motion.  RCG
      // -----------------------------------------------------------------
      foreach(var item in ReadHearing())
      {
        if (Equal(entities.Existing.OutcomeReceivedDate, local.Zero.Date))
        {
          local.HearingDtPriorToFiling.Flag = "Y";
          local.PriorHearing.Assign(entities.Existing);

          break;
        }
      }
    }
    else
    {
      // ---------------------------------------------------------
      // not an error - some ACTION TAKEN code names will not be
      // associated with Event 31
      // ---------------------------------------------------------
    }

    // ---------------------------------------------
    // Event 32 E LE ACTION ORDER FILED
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "REVIVORJ") || Equal
      (entities.LegalAction.ActionTaken, "BENCHWO") || Equal
      (entities.LegalAction.ActionTaken, "CONTEMPJ") || Equal
      (entities.LegalAction.ActionTaken, "CONTINUO") || Equal
      (entities.LegalAction.ActionTaken, "CONTINUE") || Equal
      (entities.LegalAction.ActionTaken, "GCONTINO") || Equal
      (entities.LegalAction.ActionTaken, "GARDADLO") || Equal
      (entities.LegalAction.ActionTaken, "GALAMFO") || Equal
      (entities.LegalAction.ActionTaken, "COMPENIO") || Equal
      (entities.LegalAction.ActionTaken, "NUNCPROO") || Equal
      (entities.LegalAction.ActionTaken, "JUDSTAYM") || Equal
      (entities.LegalAction.ActionTaken, "GENETICO") || Equal
      (entities.LegalAction.ActionTaken, "RELLIEN") || Equal
      (entities.LegalAction.ActionTaken, "DISMISSO") || Equal
      (entities.LegalAction.ActionTaken, "MEDSUPTJ") || Equal
      (entities.LegalAction.ActionTaken, "JENF") || Equal
      (entities.LegalAction.ActionTaken, "GENTEST") || Equal
      (entities.LegalAction.ActionTaken, "RELSATJ") || Equal
      (entities.LegalAction.ActionTaken, "CONSOLDO") || Equal
      (entities.LegalAction.ActionTaken, "SETARRSO") || Equal
      (entities.LegalAction.ActionTaken, "CRIMINAL") || Equal
      (entities.LegalAction.ActionTaken, "QUALMEDO") || Equal
      (entities.LegalAction.ActionTaken, "COMPENTE") || Equal
      (entities.LegalAction.ActionTaken, "ORDERKS") || Equal
      (entities.LegalAction.ActionTaken, "JSTAYNKS") || Equal
      (entities.LegalAction.ActionTaken, "GARNO"))
    {
      local.Infrastructure.EventId = 32;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // ---------------------------------------------
    // Event 33 E LE ACTION PETITION FILED
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "DET1PATP") || Equal
      (entities.LegalAction.ActionTaken, "DET2PATP") || Equal
      (entities.LegalAction.ActionTaken, "SUPPORTP") || Equal
      (entities.LegalAction.ActionTaken, "RIMB718P") || Equal
      (entities.LegalAction.ActionTaken, "VOLSUPPK") || Equal
      (entities.LegalAction.ActionTaken, "VOL718PK") || Equal
      (entities.LegalAction.ActionTaken, "VOLPATPK") || Equal
      (entities.LegalAction.ActionTaken, "PETCSE") || Equal
      (entities.LegalAction.ActionTaken, "CSONLYP") || Equal
      (entities.LegalAction.ActionTaken, "PATCSONP") || Equal
      (entities.LegalAction.ActionTaken, "PETITIONX") || Equal
      (entities.LegalAction.ActionTaken, "VOLGENOR"))
    {
      local.Infrastructure.EventId = 33;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // ------------------------------------------------------
    // Event 35 E LE ACTION VOLUNTARY ACTION
    // RCG  11/14/97
    // These voluntary actions have been moved to appropriate
    // Legal Classification Events.
    // No details now exist for Event 35.
    // ------------------------------------------------------
    // --------------------------------------------------------
    // Event 36 E LE DISCOVERY ACTION FILED - other Event 36's
    // will be created from the Discovery screen
    // --------------------------------------------------------
    // *** Test if event 36 is triggered.
    local.EventTriggered.Flag = "Y";

    switch(TrimEnd(entities.LegalAction.ActionTaken))
    {
      case "EOAAPMIN":
        break;
      case "EOAATY":
        break;
      case "EOAMIN":
        break;
      case "PRODDOCD":
        break;
      case "ADMREQD":
        break;
      case "INTEROGD":
        break;
      case "COMPELM":
        break;
      default:
        // *** Event 36 not triggered.
        local.EventTriggered.Flag = "";

        break;
    }

    if (AsChar(local.EventTriggered.Flag) == 'Y')
    {
      // *** Event 36 triggered.
      local.Infrastructure.EventId = 36;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // ---------------------------------------------
    // Event 02 E LE RENEWAL ACTION
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "RENEWALA"))
    {
      local.Infrastructure.EventId = 2;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // ---------------------------------------------
    // Event 27 E LE COURT HEARING OFFICER ORDERS
    // RCG - 01/15/98 turned off
    // ---------------------------------------------
    if (AsChar(entities.LegalAction.Classification) == 'C')
    {
      if (Equal(entities.LegalAction.ActionTaken, "HOMODCTJ") || Equal
        (entities.LegalAction.ActionTaken, "HOCTMPFJ") || Equal
        (entities.LegalAction.ActionTaken, "HOCTMPHJ") || Equal
        (entities.LegalAction.ActionTaken, "HOJDGFRM") || Equal
        (entities.LegalAction.ActionTaken, "HOMEDATO") || Equal
        (entities.LegalAction.ActionTaken, "HOCONFTO") || Equal
        (entities.LegalAction.ActionTaken, "HOEXPEHO") || Equal
        (entities.LegalAction.ActionTaken, "HOTERMCO") || Equal
        (entities.LegalAction.ActionTaken, "HOOTSCFO"))
      {
        local.Infrastructure.EventId = 27;
        local.Infrastructure.ReasonCode = "F" + entities
          .LegalAction.ActionTaken;
      }
    }

    // ---------------------------------------------
    // Event 38 E LE IWO ACTION
    // ---------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "IWOMODM") || Equal
      (entities.LegalAction.ActionTaken, "IWOMODO") || Equal
      (entities.LegalAction.ActionTaken, "IWOTERM") || Equal
      (entities.LegalAction.ActionTaken, "IWO") || Equal
      (entities.LegalAction.ActionTaken, "EMPANS") || Equal
      (entities.LegalAction.ActionTaken, "IWONOTKM") || Equal
      (entities.LegalAction.ActionTaken, "IWONOTKS") || Equal
      (entities.LegalAction.ActionTaken, "MEDICALA") || Equal
      (entities.LegalAction.ActionTaken, "NOIIWON") || Equal
      (entities.LegalAction.ActionTaken, "IWOISTN") || Equal
      (entities.LegalAction.ActionTaken, "IWONONKT") || Equal
      (entities.LegalAction.ActionTaken, "ENROLLM") || Equal
      (entities.LegalAction.ActionTaken, "TERMMWOM") || Equal
      (entities.LegalAction.ActionTaken, "IISSMWON") || Equal
      (entities.LegalAction.ActionTaken, "MWO") || Equal
      (entities.LegalAction.ActionTaken, "TERMMWOO") || Equal
      (entities.LegalAction.ActionTaken, "REQMWO") || Equal
      (entities.LegalAction.ActionTaken, "IWOAFF") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWOLS") || Equal
      (entities.LegalAction.ActionTaken, "NOTIWOLS"))
    {
      local.Infrastructure.EventId = 38;

      switch(TrimEnd(entities.LegalAction.ActionTaken))
      {
        case "EMPANS":
          local.Infrastructure.ReasonCode = "FI" + entities
            .LegalAction.ActionTaken;

          break;
        case "MEDICALA":
          local.Infrastructure.ReasonCode = "FI" + entities
            .LegalAction.ActionTaken;

          break;
        default:
          local.Infrastructure.ReasonCode = "F" + entities
            .LegalAction.ActionTaken;

          break;
      }
    }

    // ------------------------------------------
    // Event 96 -  LEGAL ACTION GARNISHMENT FILED
    // ------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "GARNAFFT") || Equal
      (entities.LegalAction.ActionTaken, "GARNRQW") || Equal
      (entities.LegalAction.ActionTaken, "GARNRQNW"))
    {
      local.Infrastructure.EventId = 96;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // -----------------------------------------------------
    // Event 97 -  LEGAL ACTION EOA/AFS/NOA/SERV/HRNG FILED
    // -----------------------------------------------------
    // *** Test if event 97 is triggered.
    local.EventTriggered.Flag = "Y";

    switch(TrimEnd(entities.LegalAction.ActionTaken))
    {
      case "EOAAPMIN":
        break;
      case "EOAATY":
        break;
      case "EOAMIN":
        break;
      case "EOARESP":
        break;
      case "AFFIDAVI":
        break;
      case "HEARINGN":
        break;
      case "ASSIGNLN":
        break;
      case "ASSIGNN":
        break;
      case "CHNGADDF":
        break;
      case "CHNGADDN":
        break;
      case "TERMASLN":
        break;
      case "TERMASSN":
        break;
      case "TERMPALN":
        break;
      case "TERMPARN":
        break;
      case "ASUMONSE":
        break;
      case "PRAECIPE":
        break;
      case "PROCSRVR":
        break;
      case "REQPSERV":
        break;
      case "RETSERV":
        break;
      case "SHRETSRV":
        break;
      default:
        // *** Event 97 not triggered.
        local.EventTriggered.Flag = "";

        break;
    }

    if (AsChar(local.EventTriggered.Flag) == 'Y')
    {
      // *** Event 97 triggered.
      local.Infrastructure.EventId = 97;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // ----------------------------------------------------------------
    // Event 98 - Bankruptcy:
    // Notice of filing; Confirmation of chapter 13; and Dismissal of
    // (respectively)
    // ----------------------------------------------------------------
    // *** Test if event 98 is triggered.
    local.EventTriggered.Flag = "Y";

    switch(TrimEnd(entities.LegalAction.ActionTaken))
    {
      case "BKRPNO7":
        break;
      case "BKRPNO13":
        break;
      case "BKRPCONF":
        break;
      case "BKRPDISM":
        break;
      case "BKRPDOD":
        break;
      case "BKRPPOC":
        break;
      case "BKRPMTLS":
        break;
      case "BKRPOTP":
        break;
      default:
        // *** Event 98 not triggered.
        local.EventTriggered.Flag = "";

        break;
    }

    if (AsChar(local.EventTriggered.Flag) == 'Y')
    {
      // *** Event 98 triggered.
      local.Infrastructure.EventId = 98;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // ----------------------------------------------------------------
    // Event 99 - Genetic testing filed date
    // Order for Genetic Testing, Genetic Test Order (Not KS CSE)
    // ----------------------------------------------------------------
    // *** Test if event 99 is triggered.
    local.EventTriggered.Flag = "Y";

    switch(TrimEnd(entities.LegalAction.ActionTaken))
    {
      case "GENTEST":
        break;
      case "GENETICO":
        break;
      default:
        // *** Event 99 not triggered.
        local.EventTriggered.Flag = "";

        break;
    }

    if (AsChar(local.EventTriggered.Flag) == 'Y')
    {
      // *** Event 99 triggered.
      local.Infrastructure.EventId = 99;
      local.Infrastructure.ReasonCode = "F" + entities.LegalAction.ActionTaken;
    }

    // -----------------------------
    // create Infrastructure records
    // -----------------------------
    if (local.Infrastructure.EventId > 0)
    {
      UseLeCabGetRelCaseUnitsFLact();

      for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
        .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
      {
        if (!local.RelatedCaseUnits.CheckSize())
        {
          break;
        }

        local.Infrastructure.CaseNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number;
        local.Infrastructure.CaseUnitNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCuCaseUnit.CuNumber;
        local.Infrastructure.CsePersonNumber =
          local.RelatedCaseUnits.Item.DtlRelatedObligor.Number;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
        }
      }

      local.RelatedCaseUnits.CheckIndex();
    }

    if (local.Infrastructure.EventId == 31 && AsChar
      (local.HearingDtPriorToFiling.Flag) == 'Y')
    {
      local.Infrastructure.EventId = 29;
      local.Infrastructure.UserId = "HEAR";
      local.Infrastructure.ReferenceDate = local.PriorHearing.ConductedDate;
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.CreatedTimestamp = Now();

      // ------------------------------------------------------------
      // Hearing Date has been set but Outcome has not been received.
      // ------------------------------------------------------------
      switch(TrimEnd(entities.LegalAction.ActionTaken))
      {
        case "CONTINUM":
          local.Infrastructure.ReasonCode = "HDTCONTINUM";

          break;
        case "DELTSUPM":
          local.Infrastructure.ReasonCode = "HDTDELTSUPM";

          break;
        case "CONTEMPT":
          local.Infrastructure.ReasonCode = "HDTCONTEMPT";

          break;
        case "DISMISSM":
          local.Infrastructure.ReasonCode = "HDTDISMISSM";

          break;
        case "COMPENIM":
          local.Infrastructure.ReasonCode = "HDTCOMPENIM";

          break;
        case "DEFJPATM":
          local.Infrastructure.ReasonCode = "HDTDEFJPATM";

          break;
        case "MEDESTBM":
          local.Infrastructure.ReasonCode = "HDTMEDESTBM";

          break;
        case "JUDMEXPM":
          local.Infrastructure.ReasonCode = "HDTJUDMEXPM";

          break;
        case "AIDOEXMO":
          local.Infrastructure.ReasonCode = "HDTAIDOEXMO";

          break;
        case "REVIVORM":
          local.Infrastructure.ReasonCode = "HDTREVIVORM";

          break;
        case "GALAMFM":
          local.Infrastructure.ReasonCode = "HDTGALAMFM";

          break;
        case "GARDADLM":
          local.Infrastructure.ReasonCode = "HDTGARDADLM";

          break;
        case "JDAGEMPM":
          local.Infrastructure.ReasonCode = "HDTJDAGEMPM";

          break;
        case "MODMSOM":
          local.Infrastructure.ReasonCode = "HDTMODMSOM";

          break;
        case "CSMODM":
          local.Infrastructure.ReasonCode = "HDTCSMODM";

          break;
        case "GENETICM":
          local.Infrastructure.ReasonCode = "HDTGENETICM";

          break;
        case "NUNCPROM":
          local.Infrastructure.ReasonCode = "HDTNUNCPROM";

          break;
        case "CONSOLDM":
          local.Infrastructure.ReasonCode = "HDTCONSOLDM";

          break;
        case "x":
          local.Infrastructure.ReasonCode = "HDTMOTOSTAY";

          break;
        case "718BDEFM":
          local.Infrastructure.ReasonCode = "HDT718BDEFM";

          break;
        case "JEF":
          local.Infrastructure.ReasonCode = "HDTJEF";

          break;
        case "MOTIONKS":
          local.Infrastructure.ReasonCode = "HDTMOTIONKS";

          break;
        case "COMPELM":
          local.Infrastructure.ReasonCode = "HDTCOMPELM";

          break;
        case "EXTRA3":
          break;
        case "EXTRA4":
          local.Infrastructure.ReasonCode = "";

          break;
        default:
          local.Infrastructure.ReasonCode = "HDTX";

          break;
      }

      for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
        .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
      {
        if (!local.RelatedCaseUnits.CheckSize())
        {
          break;
        }

        local.Infrastructure.CaseNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number;
        local.Infrastructure.CaseUnitNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCuCaseUnit.CuNumber;
        local.Infrastructure.CsePersonNumber =
          local.RelatedCaseUnits.Item.DtlRelatedObligor.Number;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      local.RelatedCaseUnits.CheckIndex();
    }

    // -------------------------------
    // Check and raise external alerts.
    // -------------------------------
    // --------------------------------------------------------------
    // Check and raise external alert for expedited paternity. "42"
    // --------------------------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "VOLPATTJ"))
    {
      local.ExpeditedPatLdetFound.Flag = "N";

      if (ReadLegalActionDetail1())
      {
        local.ExpeditedPatLdetFound.Flag = "Y";
      }

      if (AsChar(local.ExpeditedPatLdetFound.Flag) == 'N')
      {
        goto Test;
      }

      if (local.RelatedCaseUnits.IsEmpty)
      {
        UseLeCabGetRelCaseUnitsFLact();
      }

      // ********************************************
      // Outgoing External Agency Alert #42
      // ********************************************
      if (!local.RelatedCaseUnits.IsEmpty)
      {
        for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
          .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
        {
          if (!local.RelatedCaseUnits.CheckSize())
          {
            break;
          }

          if (IsEmpty(local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number))
          {
            continue;
          }

          if (!ReadCase2())
          {
            continue;
          }

          if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
          {
          }
          else
          {
            continue;
          }

          local.InterfaceAlert.AlertCode = "42";
          UseSpLactExternalAlert2();
        }

        local.RelatedCaseUnits.CheckIndex();
      }
    }

Test:

    // -- 05/10/17 GVandy CQ48108 (IV-D PEP changes) Remove duplicated paternity
    // logic.
    //    Will use logic in PSTEP instead.
    // --------------------------------------------------------------
    // Check and raise external alert for CS order established
    // --------------------------------------------------------------
    if (Equal(entities.LegalAction.ActionTaken, "PATERNJ") || Equal
      (entities.LegalAction.ActionTaken, "DEFJPATJ") || Equal
      (entities.LegalAction.ActionTaken, "SUPPORTJ") || Equal
      (entities.LegalAction.ActionTaken, "DFLTSUPJ") || Equal
      (entities.LegalAction.ActionTaken, "VOLPATTJ") || Equal
      (entities.LegalAction.ActionTaken, "VOLSUPTJ") || Equal
      (entities.LegalAction.ActionTaken, "MODSUPPO") || Equal
      (entities.LegalAction.ActionTaken, "JEFJ"))
    {
      foreach(var item in ReadLegalActionDetail2())
      {
        if (ReadObligationType())
        {
          if (Equal(entities.ObligationType.Code, "CS"))
          {
            if (Equal(entities.Case1.Number, "ZDEL"))
            {
              if (ReadLegalActionPerson())
              {
                // --------------------------------------------------------------
                // At least one child specified as supported person. So create
                // external alert.
                // --------------------------------------------------------------
                goto Read;
              }

              // --------------------------------------------------------------
              // Could not find a supported person. Unlikely to happen.
              // --------------------------------------------------------------
              continue;
            }
          }
          else
          {
            continue;
          }
        }
        else
        {
          continue;
        }

Read:

        UseLeCabGetRelCaseUnitsFLdet();

        if (!local.RelatedCaseUnits.IsEmpty)
        {
          for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
            .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
          {
            if (!local.RelatedCaseUnits.CheckSize())
            {
              break;
            }

            // *** PR# H00079233 - Process Open Cases only - Anand 12/06/1999
            if (ReadCase1())
            {
              if (AsChar(entities.Case1.Status) == 'C')
              {
                continue;
              }
            }

            local.InterfaceAlert.AlertCode = "43";
            UseSpLactExternalAlert1();
          }

          local.RelatedCaseUnits.CheckIndex();
        }
      }
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.ArrearsAmount = source.ArrearsAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.JudgementAmount = source.JudgementAmount;
  }

  private static void MoveRelatedCaseUnits1(LeCabGetRelCaseUnitsFLact.Export.
    RelatedCaseUnitsGroup source, Local.RelatedCaseUnitsGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.DetailRelatedCuCase.Number = source.DetailRelatedCase.Number;
    target.DetailRelatedCuCaseUnit.CuNumber =
      source.DetailRelatedCaseUnit.CuNumber;
    target.DtlRelatedObligor.Number = source.DtlRelatedObligor.Number;
  }

  private static void MoveRelatedCaseUnits2(LeCabGetRelCaseUnitsFLdet.Export.
    RelatedCaseUnitsGroup source, Local.RelatedCaseUnitsGroup target)
  {
    target.DetailRelatedCuCase.Number = source.DetailRelatedCase.Number;
    target.DetailRelatedCuCaseUnit.CuNumber =
      source.DetailRelatedCaseUnit.CuNumber;
    target.DtlRelatedObligor.Number = source.DtlRelatedObligor.Number;
    target.DtlCauSupportedCsePerson.Number = source.DtlCauSupported.Number;
    target.DtlCauSupportedLegalActionPerson.Assign(source.CauSupported);
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseLeCabGetRelCaseUnitsFLact()
  {
    var useImport = new LeCabGetRelCaseUnitsFLact.Import();
    var useExport = new LeCabGetRelCaseUnitsFLact.Export();

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;

    Call(LeCabGetRelCaseUnitsFLact.Execute, useImport, useExport);

    useExport.RelatedCaseUnits.CopyTo(
      local.RelatedCaseUnits, MoveRelatedCaseUnits1);
  }

  private void UseLeCabGetRelCaseUnitsFLdet()
  {
    var useImport = new LeCabGetRelCaseUnitsFLdet.Import();
    var useExport = new LeCabGetRelCaseUnitsFLdet.Export();

    useImport.LegalActionDetail.Number = entities.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;

    Call(LeCabGetRelCaseUnitsFLdet.Execute, useImport, useExport);

    useExport.RelatedCaseUnits.CopyTo(
      local.RelatedCaseUnits, MoveRelatedCaseUnits2);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpLactExternalAlert1()
  {
    var useImport = new SpLactExternalAlert.Import();
    var useExport = new SpLactExternalAlert.Export();

    useImport.Case1.Number =
      local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number;
    useImport.CaseUnit.CuNumber =
      local.RelatedCaseUnits.Item.DetailRelatedCuCaseUnit.CuNumber;
    useImport.LegalActionPerson.Assign(
      local.RelatedCaseUnits.Item.DtlCauSupportedLegalActionPerson);
    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    MoveLegalActionDetail(entities.LegalActionDetail,
      useImport.LegalActionDetail);
    useImport.LegalAction.Assign(entities.LegalAction);

    Call(SpLactExternalAlert.Execute, useImport, useExport);

    local.InterfaceAlert.AlertCode = useExport.InterfaceAlert.AlertCode;
  }

  private void UseSpLactExternalAlert2()
  {
    var useImport = new SpLactExternalAlert.Import();
    var useExport = new SpLactExternalAlert.Export();

    useImport.CaseUnit.CuNumber =
      local.RelatedCaseUnits.Item.DetailRelatedCuCaseUnit.CuNumber;
    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    useImport.Case1.Number = entities.Case1.Number;
    useImport.LegalAction.Assign(entities.LegalAction);

    Call(SpLactExternalAlert.Execute, useImport, useExport);

    local.InterfaceAlert.AlertCode = useExport.InterfaceAlert.AlertCode;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          local.RelatedCaseUnits.Item.DetailRelatedCuCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHearing()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Existing.ConductedDate = db.GetDate(reader, 2);
        entities.Existing.Type1 = db.GetNullableString(reader, 3);
        entities.Existing.OutcomeReceivedDate = db.GetNullableDate(reader, 4);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 4);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 8);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 2);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 3);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 2);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 3);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.Role = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 7);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of Event95ForNewLegActn.
    /// </summary>
    [JsonPropertyName("event95ForNewLegActn")]
    public Common Event95ForNewLegActn
    {
      get => event95ForNewLegActn ??= new();
      set => event95ForNewLegActn = value;
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

    private Common event95ForNewLegActn;
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
    /// <summary>A RelatedCaseUnitsGroup group.</summary>
    [Serializable]
    public class RelatedCaseUnitsGroup
    {
      /// <summary>
      /// A value of DetailRelatedCuCase.
      /// </summary>
      [JsonPropertyName("detailRelatedCuCase")]
      public Case1 DetailRelatedCuCase
      {
        get => detailRelatedCuCase ??= new();
        set => detailRelatedCuCase = value;
      }

      /// <summary>
      /// A value of DetailRelatedCuCaseUnit.
      /// </summary>
      [JsonPropertyName("detailRelatedCuCaseUnit")]
      public CaseUnit DetailRelatedCuCaseUnit
      {
        get => detailRelatedCuCaseUnit ??= new();
        set => detailRelatedCuCaseUnit = value;
      }

      /// <summary>
      /// A value of DtlRelatedObligor.
      /// </summary>
      [JsonPropertyName("dtlRelatedObligor")]
      public CsePerson DtlRelatedObligor
      {
        get => dtlRelatedObligor ??= new();
        set => dtlRelatedObligor = value;
      }

      /// <summary>
      /// A value of DtlCauSupportedCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCauSupportedCsePerson")]
      public CsePerson DtlCauSupportedCsePerson
      {
        get => dtlCauSupportedCsePerson ??= new();
        set => dtlCauSupportedCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCauSupportedLegalActionPerson.
      /// </summary>
      [JsonPropertyName("dtlCauSupportedLegalActionPerson")]
      public LegalActionPerson DtlCauSupportedLegalActionPerson
      {
        get => dtlCauSupportedLegalActionPerson ??= new();
        set => dtlCauSupportedLegalActionPerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Case1 detailRelatedCuCase;
      private CaseUnit detailRelatedCuCaseUnit;
      private CsePerson dtlRelatedObligor;
      private CsePerson dtlCauSupportedCsePerson;
      private LegalActionPerson dtlCauSupportedLegalActionPerson;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of PaternitySearch.
    /// </summary>
    [JsonPropertyName("paternitySearch")]
    public Common PaternitySearch
    {
      get => paternitySearch ??= new();
      set => paternitySearch = value;
    }

    /// <summary>
    /// A value of MaleAp.
    /// </summary>
    [JsonPropertyName("maleAp")]
    public Common MaleAp
    {
      get => maleAp ??= new();
      set => maleAp = value;
    }

    /// <summary>
    /// A value of PriorHearing.
    /// </summary>
    [JsonPropertyName("priorHearing")]
    public Hearing PriorHearing
    {
      get => priorHearing ??= new();
      set => priorHearing = value;
    }

    /// <summary>
    /// A value of HearingDtPriorToFiling.
    /// </summary>
    [JsonPropertyName("hearingDtPriorToFiling")]
    public Common HearingDtPriorToFiling
    {
      get => hearingDtPriorToFiling ??= new();
      set => hearingDtPriorToFiling = value;
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
    /// A value of ExpeditedPatLdetFound.
    /// </summary>
    [JsonPropertyName("expeditedPatLdetFound")]
    public Common ExpeditedPatLdetFound
    {
      get => expeditedPatLdetFound ??= new();
      set => expeditedPatLdetFound = value;
    }

    /// <summary>
    /// A value of EventTriggered.
    /// </summary>
    [JsonPropertyName("eventTriggered")]
    public Common EventTriggered
    {
      get => eventTriggered ??= new();
      set => eventTriggered = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// Gets a value of RelatedCaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<RelatedCaseUnitsGroup> RelatedCaseUnits =>
      relatedCaseUnits ??= new(RelatedCaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of RelatedCaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("relatedCaseUnits")]
    [Computed]
    public IList<RelatedCaseUnitsGroup> RelatedCaseUnits_Json
    {
      get => relatedCaseUnits;
      set => RelatedCaseUnits.Assign(value);
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    private CsePersonsWorkSet ap;
    private CsePerson child;
    private Common paternitySearch;
    private Common maleAp;
    private Hearing priorHearing;
    private Common hearingDtPriorToFiling;
    private DateWorkArea current;
    private Common expeditedPatLdetFound;
    private Common eventTriggered;
    private InterfaceAlert interfaceAlert;
    private Code code;
    private CodeValue codeValue;
    private Infrastructure infrastructure;
    private Array<RelatedCaseUnitsGroup> relatedCaseUnits;
    private DateWorkArea zero;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Hearing Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CsePerson Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CaseRole Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private Hearing existing;
    private CsePerson child1;
    private CaseUnit caseUnit;
    private CaseRole child2;
    private LegalActionPerson legalActionPerson;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private Case1 case1;
    private LegalAction legalAction;
  }
#endregion
}
