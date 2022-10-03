// Program: FN_CAB_RAISE_EVENT, ID: 372086104, model: 746.
// Short name: SWE01886
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
/// A program: FN_CAB_RAISE_EVENT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Written by Raju     : Dec 11 1996
/// This action block raises the event(s) for Infrastructure for following prads
/// 	BKRP, MILI, JAIL, INCH, INCS, APDS
/// </para>
/// </summary>
[Serializable]
public partial class FnCabRaiseEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_RAISE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabRaiseEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabRaiseEvent.
  /// </summary>
  public FnCabRaiseEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 01/13/97	HOOKS	initial creation
    // 12/29/97	Venkatesh Kamaraj  Deleted call to get_next_situation_no because
    // of changes to infrastructure
    // 3/25/1999 - Bud Adams  -  Got rid of ROUNDED functions;
    //   READ properties set; imported current_timestamp value
    //   Reads of interstate_request were not fully qualified.
    // 8/05/99 Maureen Brown - Added logic to generate different events
    //  depending on the legal action establishment code.  This ensures
    //  that a monitored event will be ended when an obligation is established.
    // 09/05/2000  Vithal  Madhira     -PR# 90065   Added logic to create only 
    // one infrastructure record for RECOVERY obligations. For 'IRS NEG' , the
    // event description is changed  from '1040x' to 'IRS NEG'.
    // --------------------------------------------
    // --------------------------------------------
    // Assigning global infrastructure attribute
    //   values
    // --------------------------------------------
    MoveInfrastructure(import.Infrastructure, local.Infrastructure);
    local.InfrastructureRecCreated.Flag = "N";

    if (!Equal(import.ActivityType.Text4, "DELE"))
    {
      if (!ReadObligation())
      {
        ExitState = "OBLIGATION_NOT_ACTIVE";

        return;
      }
    }

    if (import.LegalAction.Identifier != 0)
    {
      if (!ReadLegalAction())
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }
    }

    if (Equal(import.Current.Timestamp, local.Infrastructure.CreatedTimestamp))
    {
      local.Infrastructure.CreatedTimestamp = Now();
    }
    else
    {
      local.Infrastructure.CreatedTimestamp = import.Current.Timestamp;
    }

    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";

    if (Equal(import.ActivityType.Text4, "DELE"))
    {
      local.Infrastructure.ReasonCode = "OBLGDELETED";

      switch(TrimEnd(import.ObligationType.Code))
      {
        case "CS":
          local.Infrastructure.Detail =
            "CHILD SUPPORT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "SP":
          local.Infrastructure.Detail =
            "SPOUSAL SUPPORT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "MS":
          local.Infrastructure.Detail =
            "MEDICAL SUPPORT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "MC":
          local.Infrastructure.Detail =
            "MEDICAL COSTS OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "718B":
          local.Infrastructure.Detail =
            "718B URA JUDGEMENT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "AJ":
          local.Infrastructure.Detail =
            "ARREARS JUDGEMENT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "CRCH":
          local.Infrastructure.Detail =
            "COST OF RAISING A CHILD OBLIGATION TRANSACTION DELETED - AP: ";

          break;
        case "MJ":
          local.Infrastructure.Detail =
            "MEDICAL JUDGEMENT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "SAJ":
          local.Infrastructure.Detail =
            "SPOUSAL ARREARS JUDGEMENT OBLIGATION TRANSACTION DELETED - AP: ";

          break;
        case "%UME":
          local.Infrastructure.Detail =
            "% UNINSURED MEDICAL EXPENSE OBLIG TRANSACTION DELETED FOR AP: ";

          break;
        case "IVD RC":
          local.Infrastructure.Detail =
            "IV-D RECOVERY OBLIGATION TRANSACTION DELETED FOR CSE PERSON NO ";

          break;
        case "FEE":
          local.Infrastructure.Detail =
            "FEE OBLIGATION TRANSACTION DELETED FOR CSE PERSON NO: ";

          break;
        case "IJ":
          local.Infrastructure.Detail =
            "INTEREST JUDGEMENT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "BDCK RC":
          local.Infrastructure.Detail =
            "BAD CHECK RECOVERY OBLIG TRANSACTION DELETED FOR CSE PERSON NO ";

          break;
        case "IRS NEG":
          local.Infrastructure.Detail =
            "IRS NEG RECOVERY OBLIG TRANSACTION DELETED FOR CSE PERSON NO ";

          break;
        case "MIS AP":
          local.Infrastructure.Detail =
            "AP MISDIRECTED PAYMENT OBLIGATION TRANSACTION DELETED FOR AP: ";

          break;
        case "MIS AR":
          local.Infrastructure.Detail =
            "AR MISDIRECTED PAYMENT OBLIGATION TRAN DELETED - CSE PERSON NO ";

          break;
        case "MIS NON":
          local.Infrastructure.Detail =
            "NON-CSE PERS MISDIRECTED PAYMT OBLG TRAN DELETED - CSE PERS NO ";

          break;
        case "VOL":
          local.Infrastructure.Detail =
            "VOLUNTARY OBLIGATION TRANSACTION DELETED FOR CSE PERSON NO: ";

          break;
        default:
          break;
      }
    }
    else
    {
      switch(TrimEnd(import.ObligationType.Code))
      {
        case "CS":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGCSRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGCSREC";
          }

          local.Infrastructure.Detail =
            "CHILD SUPPORT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "SP":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGSPRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGSPREC";
          }

          local.Infrastructure.Detail =
            "SPOUSAL SUPPORT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "MS":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGMSRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGMSREC";
          }

          local.Infrastructure.Detail =
            "MEDICAL SUPPORT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "MC":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGMCRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGMCREC";
          }

          local.Infrastructure.Detail =
            "MEDICAL COSTS OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "718B":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLG718BRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLG718BREC";
          }

          local.Infrastructure.Detail =
            "718B URA JUDGEMENT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "AJ":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGAJRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGAJREC";
          }

          local.Infrastructure.Detail =
            "ARREARS JUDGEMENT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "CRCH":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGCRCHRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGCRCHREC";
          }

          local.Infrastructure.Detail =
            "COST OF RAISING A CHILD OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "MJ":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGMJRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGMJREC";
          }

          local.Infrastructure.Detail =
            "MEDICAL JUDGEMENT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "SAJ":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGSAJRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGSAJREC";
          }

          local.Infrastructure.Detail =
            "SPOUSAL ARREARS JUDGEMENT OBLIGATION TRANSACTION CREATED - AP: ";

          break;
        case "%UME":
          local.Infrastructure.ReasonCode = "OBLGUMEREC";
          local.Infrastructure.Detail =
            "% UNINSURED MEDICAL EXPENSE OBLIG TRANSACTION CREATED FOR AP: ";

          break;
        case "IVD RC":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGIVDRCRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGIVDRCREC";
          }

          local.Infrastructure.Detail =
            "IV-D RECOVERY OBLIGATION TRANSACTION CREATED FOR CSE PERSON NO ";

          break;
        case "FEE":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGFEERECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGFEEREC";
          }

          local.Infrastructure.Detail =
            "FEE OBLIGATION TRANSACTION CREATED FOR CSE PERSON NO: ";

          break;
        case "IJ":
          if (Equal(entities.LegalAction.EstablishmentCode, "CS") || Equal
            (entities.LegalAction.EstablishmentCode, "CT"))
          {
            local.Infrastructure.ReasonCode = "OBLGIJRECKS";
          }
          else
          {
            local.Infrastructure.ReasonCode = "OBLGIJREC";
          }

          local.Infrastructure.Detail =
            "INTEREST JUDGEMENT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "BDCK RC":
          local.Infrastructure.ReasonCode = "OBLGBDCKRCREC";
          local.Infrastructure.Detail =
            "BAD CHECK RECOVERY OBLIG TRANSACTION CREATED FOR CSE PERSON NO ";

          break;
        case "IRS NEG":
          local.Infrastructure.ReasonCode = "OBLGIRSNEGREC";
          local.Infrastructure.Detail =
            "IRS NEG RECOVERY OBLIG TRANSACTION CREATED FOR CSE PERSON NO ";

          break;
        case "MIS AP":
          local.Infrastructure.ReasonCode = "OBLGMISAPREC";
          local.Infrastructure.Detail =
            "AP MISDIRECTED PAYMENT OBLIGATION TRANSACTION CREATED FOR AP: ";

          break;
        case "MIS AR":
          local.Infrastructure.ReasonCode = "OBLGMISARREC";
          local.Infrastructure.Detail =
            "AR MISDIRECTED PAYMENT OBLIGATION TRAN CREATED - CSE PERSON NO ";

          break;
        case "MIS NON":
          local.Infrastructure.ReasonCode = "OBLGMISNONREC";
          local.Infrastructure.Detail =
            "NON-CSE PERS MISDIRECTED PAYMT OBLG TRAN CREATED - CSE PERS NO ";

          break;
        case "VOL":
          local.Infrastructure.ReasonCode = "OBLGVLNTRYREC";
          local.Infrastructure.Detail =
            "VOLUNTARY OBLIGATION TRANSACTION CREATED FOR CSE PERSON NO: ";

          break;
        default:
          break;
      }
    }

    local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + Substring
      (local.Infrastructure.CsePersonNumber, 10,
      Verify(local.Infrastructure.CsePersonNumber, "0"), 11 -
      Verify(local.Infrastructure.CsePersonNumber, "0"));

    foreach(var item in ReadCaseUnit1())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        // ---------------------------------------------
        // This is another important piece of code.
        //   - reason codes are not unique but the
        //     combination of reason code , initiating
        //     state code is unique and is used to get
        //     the correct event detail record.
        // ---------------------------------------------
        // =================================================
        // 3/26/1999 - Bud Adams  -  All Reads of Interstate_Request
        //   were only qualified by the relationship to Case.  There can
        //   be many, and even with these qualifiers there can be several.
        //   So, those Read actions have the default property.
        // =================================================
        if (ReadInterstateRequest())
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }

      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.InfrastructureRecCreated.Flag = "Y";

      if (!Equal(import.ActivityType.Text4, "DELE"))
      {
        if (ReadInfrastructure())
        {
          AssociateInfrastructure();
        }
        else
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }
      }
    }

    foreach(var item in ReadCaseUnit2())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        // ---------------------------------------------
        // This is another important piece of code.
        //   - reason codes are not unique but the
        //     combination of reason code , initiating
        //     state code is unique and is used to get
        //     the correct event detail record.
        // ---------------------------------------------
        // =================================================
        // 3/26/1999 - Bud Adams  -  All Reads of Interstate_Request
        //   were only qualified by the relationship to Case.  There can
        //   be many, and even with these qualifiers there can be several.
        //   So, those Read actions have the default property.
        // =================================================
        if (ReadInterstateRequest())
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }

      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.InfrastructureRecCreated.Flag = "Y";

      if (!Equal(import.ActivityType.Text4, "DELE"))
      {
        if (ReadInfrastructure())
        {
          AssociateInfrastructure();
        }
        else
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }
      }
    }

    if (AsChar(local.InfrastructureRecCreated.Flag) == 'N')
    {
      // ***---  Joined Case with R/E; removed Read of Case  b.a. 4/14/99
      foreach(var item in ReadCaseUnitCase1())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        // ------------------------------------------------------------------------------
        // Per PR# 90065 create only one infrastructure record for RECOVERY 
        // obligation.
        //                                         
        // Vithal Madhira (09/05/00)
        // -------------------------------------------------------------------------------
        if (AsChar(import.ObligationType.Classification) == 'R' || AsChar
          (import.ObligationType.Classification) == 'F')
        {
          local.Infrastructure.CaseUnitNumber = 0;

          if (AsChar(local.InfrastructureRecCreated.Flag) == 'Y')
          {
            return;
          }
        }

        // =================================================
        // 3/26/1999 - Bud Adams  -  All Reads of Interstate_Request
        //   were only qualified by the relationship to Case.  There can
        //   be many, and even with these qualifiers there can be several.
        //   So, those Read actions have the default property.
        // =================================================
        // : Aug 13, 1999, mfb - changed this read to default, instead of select
        // only.
        if (ReadInterstateRequest())
        {
          local.Infrastructure.InitiatingStateCode = "OS";
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

        local.InfrastructureRecCreated.Flag = "Y";

        if (!Equal(import.ActivityType.Text4, "DELE"))
        {
          if (ReadInfrastructure())
          {
            AssociateInfrastructure();
          }
          else
          {
            ExitState = "INFRASTRUCTURE_NF";

            return;
          }
        }
      }

      // =================================================
      // 4/14/99 - bud adams  -  A Recovery obligation CAN have the
      //   AR as the obligor.  If so, then we need to Read Case_Unit
      //   using a different relationship than from above.
      // =================================================
      if (AsChar(import.ObligationType.Classification) == 'R' && AsChar
        (local.InfrastructureRecCreated.Flag) == 'N')
      {
        // -------------------------------------------------------------------------------
        // Per SME, a Recovery Obligation can be created for anyone in the 
        // system. An infrastructure record must be created whenever a new
        // Recovery obligation is added.
        //                                                   
        // -- Vithal Madhira( 09/13/00)
        // ------------------------------------------------------------------------------------
        foreach(var item in ReadCaseUnitCase2())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          local.Infrastructure.CaseNumber = entities.Case1.Number;

          // ------------------------------------------------------------------------------
          // Per PR# 90065 create only one infrastructure record for RECOVERY   
          // obligation.
          //                                         
          // Vithal Madhira (09/05/00)
          // -------------------------------------------------------------------------------
          if (AsChar(import.ObligationType.Classification) == 'R')
          {
            local.Infrastructure.CaseUnitNumber = 0;

            if (AsChar(local.InfrastructureRecCreated.Flag) == 'Y')
            {
              return;
            }
          }

          // : Aug 13, 1999, mfb - changed this read to default, instead of 
          // select only.
          if (ReadInterstateRequest())
          {
            local.Infrastructure.InitiatingStateCode = "OS";
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

          local.InfrastructureRecCreated.Flag = "Y";

          if (!Equal(import.ActivityType.Text4, "DELE"))
          {
            if (ReadInfrastructure())
            {
              AssociateInfrastructure();
            }
            else
            {
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }
          }
        }

        foreach(var item in ReadCaseCaseUnit())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          local.Infrastructure.CaseNumber = entities.Case1.Number;

          // ------------------------------------------------------------------------------
          // Per PR# 90065 create only one infrastructure record for RECOVERY   
          // obligation.
          //                                         
          // Vithal Madhira (09/05/00)
          // -------------------------------------------------------------------------------
          if (AsChar(import.ObligationType.Classification) == 'R')
          {
            local.Infrastructure.CaseUnitNumber = 0;

            if (AsChar(local.InfrastructureRecCreated.Flag) == 'Y')
            {
              return;
            }
          }

          // : Aug 13, 1999, mfb - changed this read to default, instead of 
          // select only.
          if (ReadInterstateRequest())
          {
            local.Infrastructure.InitiatingStateCode = "OS";
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

          local.InfrastructureRecCreated.Flag = "Y";

          if (!Equal(import.ActivityType.Text4, "DELE"))
          {
            if (ReadInfrastructure())
            {
              AssociateInfrastructure();
            }
            else
            {
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }
          }
        }

        if (AsChar(local.InfrastructureRecCreated.Flag) == 'N')
        {
          // ------------------------------------------------------------------
          // This code is to cover the condition when creating a Recovery 
          // Obligation for 'CSE_PERSON' of any type(= C/O).
          // --------------------------------------------------------------------
          if (ReadCsePerson())
          {
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            local.InfrastructureRecCreated.Flag = "Y";

            if (!Equal(import.ActivityType.Text4, "DELE"))
            {
              if (ReadInfrastructure())
              {
                AssociateInfrastructure();
              }
              else
              {
                ExitState = "INFRASTRUCTURE_NF";
              }
            }
          }
          else
          {
            ExitState = "CSE_PERSON_NF";
          }
        }
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void AssociateInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyId = entities.Obligation.DtyGeneratedId;
    var cpaType = entities.Obligation.CpaType;
    var cspNo = entities.Obligation.CspNumber;
    var obgId = entities.Obligation.SystemGeneratedIdentifier;

    CheckValid<Infrastructure>("CpaType", cpaType);
    entities.Infrastructure.Populated = false;
    Update("AssociateInfrastructure",
      (db, command) =>
      {
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetNullableString(command, "cpaType", cpaType);
        db.SetNullableString(command, "cspNo", cspNo);
        db.SetNullableInt32(command, "obgId", obgId);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.OtyId = otyId;
    entities.Infrastructure.CpaType = cpaType;
    entities.Infrastructure.CspNo = cspNo;
    entities.Infrastructure.ObgId = obgId;
    entities.Infrastructure.Populated = true;
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CasNo);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", local.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "cspNoAr", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", local.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "cspNoChild", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitCase1()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseUnitCase1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitCase2()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseUnitCase2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAr", local.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.
          SetString(command, "numb", local.Infrastructure.CsePersonNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          local.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.OtyId = db.GetNullableInt32(reader, 1);
        entities.Infrastructure.CpaType = db.GetNullableString(reader, 2);
        entities.Infrastructure.CspNo = db.GetNullableString(reader, 3);
        entities.Infrastructure.ObgId = db.GetNullableInt32(reader, 4);
        entities.Infrastructure.Populated = true;
        CheckValid<Infrastructure>("CpaType", entities.Infrastructure.CpaType);
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
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Populated = true;
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
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ActivityType.
    /// </summary>
    [JsonPropertyName("activityType")]
    public TextWorkArea ActivityType
    {
      get => activityType ??= new();
      set => activityType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private LegalAction legalAction;
    private TextWorkArea activityType;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private CsePerson supported;
    private Infrastructure infrastructure;
    private DateWorkArea current;
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
    /// A value of InfrastructureRecCreated.
    /// </summary>
    [JsonPropertyName("infrastructureRecCreated")]
    public Common InfrastructureRecCreated
    {
      get => infrastructureRecCreated ??= new();
      set => infrastructureRecCreated = value;
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

    private Common infrastructureRecCreated;
    private Infrastructure infrastructure;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of ExistingAr.
    /// </summary>
    [JsonPropertyName("existingAr")]
    public CsePerson ExistingAr
    {
      get => existingAr ??= new();
      set => existingAr = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private LegalAction legalAction;
    private Fips fips;
    private Infrastructure infrastructure;
    private ObligationType obligationType;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private CsePerson existingAp;
    private CsePerson existingAr;
    private CsePerson existingChild;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
