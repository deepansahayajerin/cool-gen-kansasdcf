// Program: SP_INCOMING_EXT_ALRT, ID: 372050854, model: 746.
// Short name: SWE01832
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
/// A program: SP_INCOMING_EXT_ALRT.
/// </para>
/// <para>
/// This AB creates and distributes to the workers all incoming external alerts.
/// Outgoing external alerts are handled in individual ABs.
/// </para>
/// </summary>
[Serializable]
public partial class SpIncomingExtAlrt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_INCOMING_EXT_ALRT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpIncomingExtAlrt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpIncomingExtAlrt.
  /// </summary>
  public SpIncomingExtAlrt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------
    //  Date		Developer	Request #      Description
    // -------------------------------------------------------------------------------------------------------------------
    // 19 Sep 96	Michael Ramirez			Initial Dev
    // 16 Jan 97	R. Marchman			Test and rework.
    // 04/29/97	JeHoward			Current date fix
    // 07/14/97	R Grey				Add distribution by CSE Case Role
    // 08/18/97	R Grey				Utilize Event Processor for creation of OSP Alerts
    // 12/9/1998	C. Ott				Removed PERSON from READ EACH and added single READ 
    // for PERSON in
    // 						order to give more meaningful error message when read fails.
    // 04/22/09	GVandy		CQ 9788		Set denorm_text_12 on alert #1 & #22 to 1 for 
    // AR/MO role, 2 for
    // 						AP/FA role, or 3 for CH role.  This is to assist performance in the
    // 						event processor alert optimization routine.
    // -------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.CsePersonsWorkSet.Number = import.InterfaceAlert.CsePersonNumber ?? Spaces
      (10);
    export.InterfaceAlert.Identifier = import.InterfaceAlert.Identifier;

    // ****************************************************************
    //           Set up common Infrastructure Record values
    // ****************************************************************
    export.XtrnlAgencyNotfAlert.BusinessObjectCd = "CAS";
    export.XtrnlAgencyNotfAlert.CaseUnitNumber = 0;
    export.XtrnlAgencyNotfAlert.CreatedBy = "SWEPB305";
    export.XtrnlAgencyNotfAlert.CsePersonNumber =
      import.InterfaceAlert.CsePersonNumber;
    export.XtrnlAgencyNotfAlert.CsenetInOutCode = "";
    export.XtrnlAgencyNotfAlert.DenormDate = local.InitializedDate.Date;
    export.XtrnlAgencyNotfAlert.DenormText12 = "";
    export.XtrnlAgencyNotfAlert.DenormNumeric12 = 0;
    export.XtrnlAgencyNotfAlert.EventId = 24;
    export.XtrnlAgencyNotfAlert.ProcessStatus = "Q";
    export.XtrnlAgencyNotfAlert.ReferenceDate = local.Current.Date;
    export.XtrnlAgencyNotfAlert.UserId = "SWEPB305";

    // ****************************************************************
    //           Format OSP_ALERT DESCRIPTION
    // ****************************************************************
    switch(TrimEnd(import.InterfaceAlert.AlertCode))
    {
      case "01":
        export.XtrnlAgencyNotfAlert.ReasonCode = "AEADDRCHNG";
        export.XtrnlAgencyNotfAlert.Detail = "AE/IM CHANGED PERSON ADDRESS";
        export.Error.Type1 = "";

        break;
      case "02":
        export.XtrnlAgencyNotfAlert.ReasonCode = "AEPERSNMECHN";
        export.XtrnlAgencyNotfAlert.Detail = "AE/IM PERSON NAME CHANGED";

        // *********************************************
        // Format Previous Name: Last, First, MI
        // *********************************************
        local.OfficeServiceProviderAlert.Description = "PREV NAME:" + "   " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength, 1,
          17);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + ",  " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          19, 12);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + " " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          18, 1);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + ". ;   NEW:";

        // *********************************************
        // Format New Name: Last, First, MI
        // *********************************************
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + "   " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          32, 17);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + ", " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          50, 12);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + "  " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          49, 1);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + ".";
        export.Error.Type1 = "";

        break;
      case "03":
        export.XtrnlAgencyNotfAlert.ReasonCode = "NONCOOP";
        export.XtrnlAgencyNotfAlert.Detail = "AE/IM GOOD CAUSE STATUS APPROVED";
        export.Error.Type1 = "AR";

        break;
      case "04":
        export.XtrnlAgencyNotfAlert.ReasonCode = "AEPGMCHNG";
        export.XtrnlAgencyNotfAlert.Detail = "AE/IM PROGRAM TYPE CHANGE";

        // ****************************************************************
        //      Case Number
        // ****************************************************************
        local.OfficeServiceProviderAlert.Description = "AE CASE: " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength, 1,
          8);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + " ; CHILD:";

        // ****************************************************************
        //         Format Child's Name
        // ****************************************************************
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + "   " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength, 9,
          17);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + ", " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          26, 12);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + " " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          38, 1);
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + "; PLACEMENT:";
          

        // ****************************************************************
        //         Placement Change
        // ****************************************************************
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + "  " + Substring
          (import.InterfaceAlert.NoteText, InterfaceAlert.NoteText_MaxLength,
          39, 13);
        export.Error.Type1 = "CH";

        break;
      case "20":
        export.XtrnlAgencyNotfAlert.ReasonCode = "KCCASECLOSE";
        export.XtrnlAgencyNotfAlert.Detail = "KSCARES CASE CLOSED";
        export.Error.Type1 = "CH";

        break;
      case "21":
        export.XtrnlAgencyNotfAlert.ReasonCode = "KSCARENONCOOP";
        export.XtrnlAgencyNotfAlert.Detail =
          "KSCARES CHANGE IN NON COOP STATUS";
        export.Error.Type1 = "AR";

        break;
      case "22":
        export.XtrnlAgencyNotfAlert.ReasonCode = "KSCADDRCHNG";
        export.XtrnlAgencyNotfAlert.Detail = "KSCARES PERSON ADDRESS CHANGED";
        export.Error.Type1 = "";

        break;
      case "23":
        export.XtrnlAgencyNotfAlert.ReasonCode = "KSCDELETE";
        export.XtrnlAgencyNotfAlert.Detail =
          "KSCARES PERSON CASE PARTICIPANT REMOVED";
        export.Error.Type1 = "CH";

        break;
      case "24":
        export.XtrnlAgencyNotfAlert.ReasonCode = "KSCINCOMECHNG";
        export.XtrnlAgencyNotfAlert.Detail =
          "KSCARES CASE PERSON INCOME AMOUNT MODIFIED";
        export.Error.Type1 = "AP";

        break;
      default:
        // mjr THE ALERT CODE IS NOT ONE WE CURRENTLY USE
        ExitState = "ACO_NE0000_INVALID_CODE";

        return;
    }

    local.AlertNumber.Count =
      (int)StringToNumber(import.InterfaceAlert.AlertCode);

    if (ReadAlert())
    {
      local.OfficeServiceProviderAlert.TypeCode = "AUT";
      local.OfficeServiceProviderAlert.Message = entities.Alert.Message;
      local.OfficeServiceProviderAlert.DistributionDate = local.Current.Date;
      local.OfficeServiceProviderAlert.RecipientUserId = "";
      local.OfficeServiceProviderAlert.CreatedBy = "SWEPB305";
      local.OfficeServiceProviderAlert.CreatedTimestamp =
        local.Current.Timestamp;

      // ***********************************************
      // If no Alert description was defined previously, set descriptive text to
      // identify CSE Person Number and Case ID.
      // ***********************************************
      if (IsEmpty(local.OfficeServiceProviderAlert.Description))
      {
        local.OfficeServiceProviderAlert.Description =
          TrimEnd(local.OfficeServiceProviderAlert.Description) + "  CSE PERSON NUMBER:   " +
          TrimEnd(import.InterfaceAlert.CsePersonNumber);

        switch(TrimEnd(import.InterfaceAlert.AlertCode))
        {
          case "20":
            local.OfficeServiceProviderAlert.Description =
              TrimEnd(local.OfficeServiceProviderAlert.Description) + "   KSCARES CASE NO:  " +
              Substring
              (import.InterfaceAlert.NoteText,
              InterfaceAlert.NoteText_MaxLength, 1, 8);

            break;
          case "21":
            local.OfficeServiceProviderAlert.Description =
              TrimEnd(local.OfficeServiceProviderAlert.Description) + "   KSCARES CASE NO:  " +
              Substring
              (import.InterfaceAlert.NoteText,
              InterfaceAlert.NoteText_MaxLength, 1, 8);

            break;
          case "22":
            break;
          case "23":
            local.OfficeServiceProviderAlert.Description =
              TrimEnd(local.OfficeServiceProviderAlert.Description) + "   KSCARES CASE NO:  " +
              Substring
              (import.InterfaceAlert.NoteText,
              InterfaceAlert.NoteText_MaxLength, 1, 8);

            break;
          case "24":
            break;
          default:
            break;
        }
      }

      export.XtrnlAgencyNotfAlert.Detail =
        local.OfficeServiceProviderAlert.Description ?? "";
    }
    else
    {
      ExitState = "SP0000_ALERT_NF";

      return;
    }

    // ***********************************************
    // Add logic to create the Infrastructure record.  RCG - 7/21/97
    // Remove reference to situation no.  RCG - 11/26/97
    // ***********************************************
    // ****************************************************************
    // 12/9/1998     C. Ott     Removed PERSON from READ EACH and added single 
    // READ for PERSON in order to give more meaningful error message when read
    // fails.
    // ****************************************************************
    local.Count.Count = 0;

    if (ReadCsePerson())
    {
      foreach(var item in ReadCaseCaseRole())
      {
        // ****************************************************************
        // Check for additional Roles for the Person on the same Case so that 
        // the Alert is not generated more than once.
        // ****************************************************************
        if (Equal(local.CsePersonsWorkSet.Number,
          local.PrevCaseRole.PrevPersNumb.Number))
        {
          if (Equal(entities.Case1.Number,
            local.PrevCaseRole.PrevCaseNumb.Number))
          {
            continue;
          }
        }

        // ***********************************************
        // Business Rules for generation and distribution based upon the Case 
        // Role played by the Person identified in the External Notification.
        // ***********************************************
        switch(TrimEnd(entities.CaseRole.Type1))
        {
          case "AP":
            // ****************************************************************
            // For AP Case Role, Alert Codes 01, 02, 22 and 24 are valid.
            // ****************************************************************
            switch(TrimEnd(import.InterfaceAlert.AlertCode))
            {
              case "01":
                export.XtrnlAgencyNotfAlert.DenormText12 = "2";

                break;
              case "02":
                break;
              case "03":
                continue;
              case "04":
                continue;
              case "20":
                continue;
              case "21":
                continue;
              case "22":
                export.XtrnlAgencyNotfAlert.DenormText12 = "2";

                break;
              case "23":
                continue;
              case "24":
                break;
              default:
                break;
            }

            break;
          case "AR":
            // ****************************************************************
            // For AR Case Role, Alert Codes 01, 02, 03, 21 and 22 are valid.
            // ****************************************************************
            switch(TrimEnd(import.InterfaceAlert.AlertCode))
            {
              case "01":
                export.XtrnlAgencyNotfAlert.DenormText12 = "1";

                break;
              case "02":
                break;
              case "03":
                break;
              case "04":
                continue;
              case "20":
                continue;
              case "21":
                break;
              case "22":
                export.XtrnlAgencyNotfAlert.DenormText12 = "1";

                break;
              case "23":
                continue;
              case "24":
                continue;
              default:
                break;
            }

            break;
          case "CH":
            // ****************************************************************
            // For CH Case Role, Alert Codes 01, 02, 04, 20, 22 and 23 are 
            // valid.
            // ****************************************************************
            switch(TrimEnd(import.InterfaceAlert.AlertCode))
            {
              case "01":
                export.XtrnlAgencyNotfAlert.DenormText12 = "3";

                break;
              case "02":
                break;
              case "03":
                continue;
              case "04":
                break;
              case "20":
                break;
              case "21":
                continue;
              case "22":
                export.XtrnlAgencyNotfAlert.DenormText12 = "3";

                break;
              case "23":
                break;
              case "24":
                continue;
              default:
                break;
            }

            break;
          case "FA":
            // ****************************************************************
            // For FA Case Role, Alert Codes 01, 02, and 22 are valid.
            // ****************************************************************
            switch(TrimEnd(import.InterfaceAlert.AlertCode))
            {
              case "01":
                export.XtrnlAgencyNotfAlert.DenormText12 = "2";

                break;
              case "02":
                break;
              case "03":
                continue;
              case "04":
                continue;
              case "20":
                continue;
              case "21":
                continue;
              case "22":
                export.XtrnlAgencyNotfAlert.DenormText12 = "2";

                break;
              case "23":
                continue;
              case "24":
                continue;
              default:
                break;
            }

            break;
          case "MO":
            // ****************************************************************
            // For MO Case Role, Alert Codes 01, 02, and 22 are valid.
            // ****************************************************************
            switch(TrimEnd(import.InterfaceAlert.AlertCode))
            {
              case "01":
                export.XtrnlAgencyNotfAlert.DenormText12 = "1";

                break;
              case "02":
                break;
              case "22":
                export.XtrnlAgencyNotfAlert.DenormText12 = "1";

                break;
              default:
                continue;
            }

            break;
          default:
            break;
        }

        // ************************************************
        // Track Case Roles to avoid duplicate History.
        // ************************************************
        local.PrevCaseRole.PrevCaseNumb.Number = entities.Case1.Number;
        local.PrevCaseRole.PrevPersNumb.Number = local.CsePersonsWorkSet.Number;
        export.XtrnlAgencyNotfAlert.CaseNumber = entities.Case1.Number;
        ++local.Count.Count;

        if (ReadInterstateRequest())
        {
          switch(AsChar(entities.InterstateRequest.KsCaseInd))
          {
            case 'Y':
              export.XtrnlAgencyNotfAlert.InitiatingStateCode = "KS";

              break;
            case 'N':
              export.XtrnlAgencyNotfAlert.InitiatingStateCode = "OS";

              break;
            default:
              export.XtrnlAgencyNotfAlert.InitiatingStateCode = "OS";

              break;
          }
        }
        else
        {
          export.XtrnlAgencyNotfAlert.InitiatingStateCode = "KS";
        }

        export.XtrnlAgencyNotfAlert.SituationNumber = 0;
        UseSpCabCreateInfrastructure();

        if (IsExitState("SP0000_EVENT_DETAIL_NF"))
        {
          return;
        }

        ++import.ExpCntlTotNumAlrtsCreat.Count;
      }

      if (local.Count.Count == 0)
      {
        ExitState = "CASE_ROLE_NF";
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(export.XtrnlAgencyNotfAlert);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    export.XtrnlAgencyNotfAlert.Assign(useExport.Infrastructure);
  }

  private bool ReadAlert()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert",
      (db, command) =>
      {
        db.SetInt32(command, "count", local.AlertNumber.Count);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
        entities.Alert.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 4);
        entities.CaseRole.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Identifier = db.GetInt32(reader, 6);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 8);
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
        db.SetString(command, "numb", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of ExpCntlTotNumAlrtsCreat.
    /// </summary>
    [JsonPropertyName("expCntlTotNumAlrtsCreat")]
    public Common ExpCntlTotNumAlrtsCreat
    {
      get => expCntlTotNumAlrtsCreat ??= new();
      set => expCntlTotNumAlrtsCreat = value;
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
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private Common expCntlTotNumAlrtsCreat;
    private ProgramProcessingInfo programProcessingInfo;
    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public CaseRole Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of XtrnlAgencyNotfAlert.
    /// </summary>
    [JsonPropertyName("xtrnlAgencyNotfAlert")]
    public Infrastructure XtrnlAgencyNotfAlert
    {
      get => xtrnlAgencyNotfAlert ??= new();
      set => xtrnlAgencyNotfAlert = value;
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

    private CaseRole error;
    private Infrastructure xtrnlAgencyNotfAlert;
    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PrevCaseRoleGroup group.</summary>
    [Serializable]
    public class PrevCaseRoleGroup
    {
      /// <summary>
      /// A value of PrevPersNumb.
      /// </summary>
      [JsonPropertyName("prevPersNumb")]
      public CsePersonsWorkSet PrevPersNumb
      {
        get => prevPersNumb ??= new();
        set => prevPersNumb = value;
      }

      /// <summary>
      /// A value of PrevCaseNumb.
      /// </summary>
      [JsonPropertyName("prevCaseNumb")]
      public CsePersonsWorkSet PrevCaseNumb
      {
        get => prevCaseNumb ??= new();
        set => prevCaseNumb = value;
      }

      private CsePersonsWorkSet prevPersNumb;
      private CsePersonsWorkSet prevCaseNumb;
    }

    /// <summary>
    /// Gets a value of PrevCaseRole.
    /// </summary>
    [JsonPropertyName("prevCaseRole")]
    public PrevCaseRoleGroup PrevCaseRole
    {
      get => prevCaseRole ?? (prevCaseRole = new());
      set => prevCaseRole = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of AlertNumber.
    /// </summary>
    [JsonPropertyName("alertNumber")]
    public Common AlertNumber
    {
      get => alertNumber ??= new();
      set => alertNumber = value;
    }

    private PrevCaseRoleGroup prevCaseRole;
    private Common count;
    private DateWorkArea initializedDate;
    private Infrastructure infrastructure;
    private DateWorkArea current;
    private CsePersonAddress csePersonAddress;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Common alertNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Infrastructure Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private InterstateRequest interstateRequest;
    private Infrastructure existing;
    private Program program;
    private PersonProgram personProgram;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Alert alert;
  }
#endregion
}
