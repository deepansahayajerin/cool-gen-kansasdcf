// Program: FN_CAB_RAISE_EVENT_PAID_OFF, ID: 372449910, model: 746.
// Short name: SWE01889
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
/// A program: FN_CAB_RAISE_EVENT_PAID_OFF.
/// </para>
/// <para>
/// raises an event if a debt is paid off
/// </para>
/// </summary>
[Serializable]
public partial class FnCabRaiseEventPaidOff: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_RAISE_EVENT_PAID_OFF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabRaiseEventPaidOff(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabRaiseEventPaidOff.
  /// </summary>
  public FnCabRaiseEventPaidOff(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // Date		Author		Remarks
    // 01/15/97	Tanmoy		initial creation
    // 07/28/99	Srini Ganji    	Pick 'AR' or 'AP' Case units for CSE_PERSON
    // 08/10/99	Srini Ganji    	Special Process for Recovery type obligations.
    // 08/25/99	Srini Ganji    	PR#234, Find all cases associated to the case 
    // units for the paid in full obligation and create an infrastructure
    // record.
    // 09/20/99	Srini Ganji     Added new Local View for CASE
    // 09/22/99	Srini Ganji     Find the Case for Supported using Debt Detail 
    // duedate
    // 02/21/00	Srini Ganji     PR#88196 - Code added to Raise an event for CRU 
    // worker for paid off recovery obligations
    // 01/19/01	Madhu Kumar	PR#106748 A - Infrastructure record must have a case
    // number .
    // 04/05/06	GVandy		PR#261670 New logic for identifying case units for which
    // the event(s) will be raised.
    // *****************************************************************
    export.ErrorFound.ActionEntry = "";
    export.ErrorFound.Flag = "N";
    export.NoOfInfraRecsCreated.Count = 0;

    // *****************************************************************
    // Assigning global infrastructure attribute  values
    // *****************************************************************
    local.Infrastructure.Assign(import.Infrastructure);
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";

    switch(TrimEnd(import.ObligationType.Code))
    {
      case "FEE":
        local.Infrastructure.ReasonCode = "OBLGAPFEEPD";
        local.Infrastructure.Detail = "FEE OBLIGATION PAID OFF ";

        break;
      case "CS":
        local.Infrastructure.ReasonCode = "OBLGCSPD";
        local.Infrastructure.Detail =
          "CHILD SUPPORT OBLIGATION TRANSACTION PAID OFF FOR THE SUPPORTED ";

        break;
      case "SP":
        local.Infrastructure.ReasonCode = "OBLGSPPD";
        local.Infrastructure.Detail =
          "SPOUSAL SUPPORT OBLIGATION TRANSACTION PAID OFF FOR SUPPORTED ";

        break;
      case "MS":
        local.Infrastructure.ReasonCode = "OBLGMSPD";
        local.Infrastructure.Detail =
          "MEDICAL SUPPORT OBLIGATION TRANSACTION PAID OFF FOR SUPPORTED ";

        break;
      case "SAJ":
        local.Infrastructure.ReasonCode = "OBLGSAJPD";
        local.Infrastructure.Detail =
          "SPOUSAL SUPPORT ARREARS JUDGEMENT PAID OFF FOR SUPPORTED";

        break;
      case "IVD RC":
        local.Infrastructure.ReasonCode = "OBLGIVDRCPD";
        local.Infrastructure.Detail = "IVD RECOVERY PAID OFF";

        break;
      case "IJ":
        local.Infrastructure.ReasonCode = "OBLGIJPD";
        local.Infrastructure.Detail = "INTEREST JUDGEMENT PAID OFF";

        break;
      case "MIS AR":
        local.Infrastructure.ReasonCode = "OBLGMISARPD";
        local.Infrastructure.Detail = "AR MISDIRECTED PAYMENT PAID OFF";

        break;
      case "CRCH":
        local.Infrastructure.ReasonCode = "OBLGCRCHPD";
        local.Infrastructure.Detail = "COST OF RAISING CHILD PAID OFF";

        break;
      case "MC":
        local.Infrastructure.ReasonCode = "OBLGMCPD";
        local.Infrastructure.Detail = "MEDICAL COSTS PAID OFF";

        break;
      case "IRS NEG":
        local.Infrastructure.ReasonCode = "OBLGIRSNEGPD";
        local.Infrastructure.Detail = "IRS NEG RECOVERY PAID OFF";

        break;
      case "MIS AP":
        local.Infrastructure.ReasonCode = "OBLGMISAPPD";
        local.Infrastructure.Detail = "AP MISDIRECTED PAYMENT PAID OFF";

        break;
      case "MIS NON":
        local.Infrastructure.ReasonCode = "OBLGMISNONPD";
        local.Infrastructure.Detail =
          "NON-CSE PERSON MISDIRECTED PAYMENT PAID OFF";

        break;
      case "BDCK RC":
        local.Infrastructure.ReasonCode = "OBLGBDCKRCPD";
        local.Infrastructure.Detail = "BAD CHECK PAID OFF";

        break;
      case "MJ":
        local.Infrastructure.ReasonCode = "OBLGMJPD";
        local.Infrastructure.Detail = "MEDICAL JUDGEMENT PAID OFF";

        break;
      case "%UME":
        local.Infrastructure.ReasonCode = "OBLGUMEPD";
        local.Infrastructure.Detail =
          "% UNINSURED MEDICAL EXP JUDGEMENT PAID OFF";

        break;
      case "AJ":
        local.Infrastructure.ReasonCode = "OBLGAJPD";
        local.Infrastructure.Detail = "ARREARS JUDGEMENT PAID OFF";

        break;
      case "718B":
        local.Infrastructure.ReasonCode = "OBLG718BPD";
        local.Infrastructure.Detail = "718B URA JUDGEMENT PAID OFF";

        break;
      default:
        break;
    }

    local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " " + Substring
      (local.Infrastructure.CsePersonNumber, 10,
      Verify(local.Infrastructure.CsePersonNumber, "0"), 11 -
      Verify(local.Infrastructure.CsePersonNumber, "0"));

    if (AsChar(import.Action.Flag) == 'A')
    {
      local.Infrastructure.ReasonCode = "FNOBGREACT";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "- REACTIVATED";
        
    }

    // *****************************************************************
    // Check Obligation type for Recovery or 'FEE' type
    // *****************************************************************
    local.Recovery.Flag = "N";
    local.Fee.Flag = "N";

    if (Equal(import.ObligationType.Code, "FEE"))
    {
      local.Fee.Flag = "Y";
    }
    else if (AsChar(import.ObligationType.Classification) == 'R')
    {
      local.Recovery.Flag = "Y";
    }

    // *****************************************************************
    // For Recovery and 'FEE' type obligations, Create an
    // Infrastructure record for Obligor, No Case or Case Unit will
    // be associated to the Event. Case # and Case Unit will be
    // Blank - PR#234 - 08/25/99
    // *****************************************************************
    // ******************************************************************
    //  We will not create any infrastructure records for the FEE
    // and RECovery obligations as per PR # 106748 A .
    // The FEE and the RECovery obligations were erroring off
    // since there are no case or case units associated with them.
    // Hence the code below is commented out.
    // ---    19th January 2001.
    // ******************************************************************
    // *****************************************************************
    // Per PR# 114578  Alert and History records need to be created when 
    // recovery obligations are paid off.
    //                                                
    // Vithal (02/23/2001)
    // *****************************************************************
    if (AsChar(local.Fee.Flag) == 'Y')
    {
      return;
    }

    if (AsChar(local.Recovery.Flag) == 'Y')
    {
      local.Infrastructure.CaseUnitNumber = 0;
      local.Infrastructure.CaseNumber = "";
      local.Infrastructure.InitiatingStateCode = "KS";

      if (ReadInterstateRequestObligationInterstateRequest())
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

      local.Infrastructure.SystemGeneratedIdentifier = 0;
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ErrorFound.Flag = "Y";

        if (IsExitState("SP0000_EVENT_DETAIL_NF"))
        {
          export.ErrorFound.ActionEntry = "03";
        }
        else
        {
          export.ErrorFound.ActionEntry = "99";
        }

        return;
      }

      ++export.NoOfInfraRecsCreated.Count;

      // *****************************************************************
      // Code added for PR#88196 - 02/21/00
      // *****************************************************************
      if (AsChar(import.ObligationType.Classification) == 'R')
      {
        if (!ReadObligationObligationType())
        {
          export.ErrorFound.Flag = "Y";
          export.ErrorFound.ActionEntry = "04";
          ExitState = "FN0000_OBLIGATION_NF";

          return;
        }

        if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
        {
          export.ErrorFound.Flag = "Y";
          export.ErrorFound.ActionEntry = "10";

          return;
        }

        if (ReadInfrastructure())
        {
          AssociateInfrastructure();
        }
        else
        {
          export.ErrorFound.Flag = "Y";
          export.ErrorFound.ActionEntry = "09";
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }
      }

      // *****************************************************************
      // End of Code added for PR#88196 - 02/21/00
      // *****************************************************************
      return;
    }

    // 04/05/06  GVandy  PR#261670 New logic for identifying case units for 
    // which the event(s) will be raised.
    foreach(var item in ReadCsePerson())
    {
      // -- Read Each Case/Case unit for Supported Person
      if (Equal(import.ObligationType.Code, "SP") || Equal
        (import.ObligationType.Code, "SAJ"))
      {
        // -- If Obligation type is 'SP'/ 'SAJ' read AP/AR Case units
        foreach(var item1 in ReadCaseCaseUnit1())
        {
          // -- Only raise an event on this case if the AP still has an active 
          // case role.
          if (!ReadCaseRole())
          {
            continue;
          }

          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

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

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++export.NoOfInfraRecsCreated.Count;
          }
          else if (IsExitState("SP0000_EVENT_DETAIL_NF"))
          {
            export.ErrorFound.ActionEntry = "03";
            export.ErrorFound.Flag = "Y";

            return;
          }
          else
          {
            export.ErrorFound.Flag = "Y";
            export.ErrorFound.ActionEntry = "99";

            return;
          }
        }
      }
      else
      {
        // -- For all other Obligation type read 'CH' Case unit
        foreach(var item1 in ReadCaseCaseUnit2())
        {
          // -- Only raise an event on this case if the AP still has an active 
          // case role.
          if (!ReadCaseRole())
          {
            continue;
          }

          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

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

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++export.NoOfInfraRecsCreated.Count;
          }
          else if (IsExitState("SP0000_EVENT_DETAIL_NF"))
          {
            export.ErrorFound.ActionEntry = "03";
            export.ErrorFound.Flag = "Y";

            return;
          }
          else
          {
            export.ErrorFound.Flag = "Y";
            export.ErrorFound.ActionEntry = "99";

            return;
          }
        }
      }
    }

    // --  End New logic for identifying case units for which the event(s) will 
    // be raised.
  }

  private static void MoveInfrastructure(Infrastructure source,
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

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

  private IEnumerable<bool> ReadCaseCaseUnit1()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAr", entities.SupportedCsePerson.Number);
        db.SetNullableString(
          command, "cspNoAp", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 5);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit2()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild", entities.SupportedCsePerson.Number);
        db.SetNullableString(
          command, "cspNoAp", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 5);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Processing.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.SupportedCsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;

        return true;
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
        db.SetNullableString(
          command, "casINumber", local.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequestObligationInterstateRequest()
  {
    entities.InterstateRequestObligation.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestObligationInterstateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "orderEndDate", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.InterstateRequestObligation.OtyType = db.GetInt32(reader, 0);
        entities.InterstateRequestObligation.CpaType = db.GetString(reader, 1);
        entities.InterstateRequestObligation.CspNumber =
          db.GetString(reader, 2);
        entities.InterstateRequestObligation.ObgGeneratedId =
          db.GetInt32(reader, 3);
        entities.InterstateRequestObligation.IntGeneratedId =
          db.GetInt32(reader, 4);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 4);
        entities.InterstateRequestObligation.OrderEndDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 6);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 7);
        entities.InterstateRequestObligation.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequestObligation>("CpaType",
          entities.InterstateRequestObligation.CpaType);
      });
  }

  private bool ReadObligationObligationType()
  {
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationType.Code = db.GetString(reader, 4);
        entities.ObligationType.Classification = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of Processing.
    /// </summary>
    [JsonPropertyName("processing")]
    public DateWorkArea Processing
    {
      get => processing ??= new();
      set => processing = value;
    }

    /// <summary>
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public Common Action
    {
      get => action ??= new();
      set => action = value;
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

    private DateWorkArea processing;
    private Common action;
    private Infrastructure infrastructure;
    private Obligation obligation;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NoOfInfraRecsCreated.
    /// </summary>
    [JsonPropertyName("noOfInfraRecsCreated")]
    public Common NoOfInfraRecsCreated
    {
      get => noOfInfraRecsCreated ??= new();
      set => noOfInfraRecsCreated = value;
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

    private Common noOfInfraRecsCreated;
    private Common errorFound;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Fee.
    /// </summary>
    [JsonPropertyName("fee")]
    public Common Fee
    {
      get => fee ??= new();
      set => fee = value;
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
    /// A value of Recovery.
    /// </summary>
    [JsonPropertyName("recovery")]
    public Common Recovery
    {
      get => recovery ??= new();
      set => recovery = value;
    }

    private DateWorkArea null1;
    private Common fee;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
    private Common recovery;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    private CsePerson obligor1;
    private CsePerson apCsePerson;
    private CsePerson supportedCsePerson;
    private Infrastructure infrastructure;
    private ObligationTransaction obligationTransaction;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CaseRole apCaseRole;
    private InterstateRequestObligation interstateRequestObligation;
    private InterstateRequest interstateRequest;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor2;
    private CsePersonAccount supportedCsePersonAccount;
  }
#endregion
}
