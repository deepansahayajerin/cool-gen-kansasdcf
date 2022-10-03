// Program: OE_B413_IS_CSI_R_NEEDED, ID: 373437109, model: 746.
// Short name: SWE01963
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B413_IS_CSI_R_NEEDED.
/// </summary>
[Serializable]
public partial class OeB413IsCsiRNeeded: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B413_IS_CSI_R_NEEDED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB413IsCsiRNeeded(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB413IsCsiRNeeded.
  /// </summary>
  public OeB413IsCsiRNeeded(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************************************
    // 04/29/2005  PR243238  Ed Lyman  Fix problem that allowed ap to slip 
    // through here and be
    // not found in Create IS Request.  Changed the read each to qualify only if
    // case role end
    // date greater than (rather than greater or equal) current date.
    // ****************************************************************************************
    // ****************************************************************************************
    // 04/29/2005  PR243238  Ed Lyman  Prevent cse person not found from causing
    // an abend.
    //                                 
    // Just report it as an error.
    // ****************************************************************************************
    export.Commit.Count = import.Commit.Count;
    export.InterstateCaseExists.Count = import.InterstateCaseExists.Count;
    export.PromatchNonCsenet.Count = import.PromatchNonCsenet.Count;
    export.PromatchAlreadyReceived.Count = import.PromatchAlreadyReceived.Count;
    export.PromatchCreated.Count = import.PromatchCreated.Count;
    export.CsiRequest.Count = import.CsenetCsiRequest.Count;

    // *******************************************************************
    // If the other state's case is being closed, according to the
    // proactive match record, don't request information.
    // *******************************************************************
    // *******************************************************************
    // The user field which previously was unused now contains useful
    // information.  When action code is C for case record, the user field
    // will contain the change type (1-4).  When action code is P for person
    // record, the user field will contain the delete indicator (C or P).
    // *******************************************************************
    switch(TrimEnd(import.FcrProactiveMatchResponse.UserField ?? ""))
    {
      case "1":
        // ****************************************
        // Case change from non IV-D to IV-D
        // ****************************************
        break;
      case "2":
        // ****************************************
        // Case ID Change
        // ****************************************
        break;
      case "3":
        // ****************************************
        // Court Order Change (N to Y)
        // ****************************************
        break;
      case "4":
        // ****************************************
        // Case was Deleted
        // ****************************************
        return;
      case "P":
        // ****************************************
        // Person Deleted from Case
        // ****************************************
        break;
      case "C":
        // ****************************************
        // Case that this person was on was Deleted
        // ****************************************
        return;
      default:
        break;
    }

    // *************************************************
    // Determine if other state can handle CSI requests.
    // *************************************************
    if (!ReadCodeValue())
    {
      ++export.PromatchNonCsenet.Count;

      // ************************************************
      // Send alert to each worker to manually request
      // case information from the other state.
      // ************************************************
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.EventId = 10;
      local.Infrastructure.UserId = "FCR";
      local.Infrastructure.ReasonCode = "FPLSRCV";
      local.Infrastructure.BusinessObjectCd = "FPL";
      local.Infrastructure.DenormNumeric12 =
        import.FcrProactiveMatchResponse.Identifier;
      local.Infrastructure.ReferenceDate =
        import.FcrProactiveMatchResponse.DateReceived;
      local.ConvertDate.Date = import.FcrProactiveMatchResponse.DateReceived;
      UseCabConvertDate2String();
      local.Infrastructure.Detail =
        "Proactive match from state not CSI ready - manual contact required.";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
      local.Infrastructure.InitiatingStateCode = "OS";

      foreach(var item in ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.Commit.Count;
      }

      return;
    }

    // We are only interested in AP's.
    // ********************************************************************
    // The interstate request entity does not record the other state's
    // person number.  Only one case per other state is allowed per CSE
    // Person. (For example, a CSE person can have only one case in Texas,
    // but could also have one case in Utah and one case in Iowa).  The
    // following read is intended to find if we have interstate involvement
    // between the CSE Person and the State in question, regardless of our
    // case number or the other state's case number.
    // ********************************************************************
    local.InterstateRequest.OtherStateFips =
      (int)StringToNumber(import.FcrProactiveMatchResponse.
        TransmitterStateOrTerrCode);

    if (ReadInterstateRequest())
    {
      if (!IsEmpty(entities.InterstateRequest.KsCaseInd) && AsChar
        (entities.InterstateRequest.OtherStateCaseStatus) != 'C')
      {
        // ***********************************************************
        // If there is interstate involvement and the other state's
        // case is open, then no action needed.
        // KS Case Indicator  Y = outgoing case (KS has AR and kids)
        //                    N = incoming case (KS has the AP)
        //                space = no interstate involvement
        // If no involvement, we need to send request to other state
        // and based on their response, the worker can determine if
        // involvement is necessary.
        // ***********************************************************
        ++export.InterstateCaseExists.Count;

        return;
      }
    }

    // ****************************************************************************************
    // 04/29/2005  PR243238 Ed Lyman  Fix problem that allowed ap to slip 
    // through here and be
    // not found in Create IS Request.  Changed the read each to qualify only if
    // case role end
    // date greater than (rather than greater or equal) current date.
    // ****************************************************************************************
    foreach(var item in ReadCaseRoleCase())
    {
      UseOeB413CreateCsiRTransaction();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CSE_PERSON_NF"))
        {
          // ****************************************************************************************
          // 04/29/2005  PR243238  Ed Lyman  Prevent cse person not found from 
          // causing an abend.
          //                                 
          // Just report it as an error.
          // ****************************************************************************************
          ++export.CsePersonNotFound.Count;
          local.EabReportSend.RptDetail =
            "Create FCR Proactive Match Response cse person not found: " + (
              local.FcrProactiveMatchResponse.StateMemberId ?? "");
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("CO0000_ABSENT_PARENT_NF"))
        {
          // ****************************************************************************************
          // 04/29/2005  PR243238  Ed Lyman  Prevent cse person not found from 
          // causing an abend.
          //                                 
          // Just report it as an error.
          // ****************************************************************************************
          ++export.CsePersonNotFound.Count;
          local.EabReportSend.RptDetail =
            "Create FCR Proactive Match Response absent parent not found for person: " +
            (local.FcrProactiveMatchResponse.StateMemberId ?? "");
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else
        {
          return;
        }
      }

      ++export.CsiRequest.Count;
      ++export.Commit.Count;
    }
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

    useImport.DateWorkArea.Date = local.ConvertDate.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeB413CreateCsiRTransaction()
  {
    var useImport = new OeB413CreateCsiRTransaction.Import();
    var useExport = new OeB413CreateCsiRTransaction.Export();

    useImport.FcrProactiveMatchResponse.
      Assign(import.FcrProactiveMatchResponse);
    useImport.Case1.Number = entities.Case1.Number;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(OeB413CreateCsiRTransaction.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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

        return true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Csi.Id);
        db.SetNullableString(
          command, "transmitterStateOrTerrCode",
          import.FcrProactiveMatchResponse.TransmitterStateOrTerrCode ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "othrStateFipsCd", local.InterstateRequest.OtherStateFips);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 3);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 7);
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
    /// A value of CsePersonNotFound.
    /// </summary>
    [JsonPropertyName("csePersonNotFound")]
    public Common CsePersonNotFound
    {
      get => csePersonNotFound ??= new();
      set => csePersonNotFound = value;
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
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of InterstateCaseExists.
    /// </summary>
    [JsonPropertyName("interstateCaseExists")]
    public Common InterstateCaseExists
    {
      get => interstateCaseExists ??= new();
      set => interstateCaseExists = value;
    }

    /// <summary>
    /// A value of PromatchNonCsenet.
    /// </summary>
    [JsonPropertyName("promatchNonCsenet")]
    public Common PromatchNonCsenet
    {
      get => promatchNonCsenet ??= new();
      set => promatchNonCsenet = value;
    }

    /// <summary>
    /// A value of PromatchAlreadyReceived.
    /// </summary>
    [JsonPropertyName("promatchAlreadyReceived")]
    public Common PromatchAlreadyReceived
    {
      get => promatchAlreadyReceived ??= new();
      set => promatchAlreadyReceived = value;
    }

    /// <summary>
    /// A value of PromatchCreated.
    /// </summary>
    [JsonPropertyName("promatchCreated")]
    public Common PromatchCreated
    {
      get => promatchCreated ??= new();
      set => promatchCreated = value;
    }

    /// <summary>
    /// A value of CsenetCsiRequest.
    /// </summary>
    [JsonPropertyName("csenetCsiRequest")]
    public Common CsenetCsiRequest
    {
      get => csenetCsiRequest ??= new();
      set => csenetCsiRequest = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Csi.
    /// </summary>
    [JsonPropertyName("csi")]
    public Code Csi
    {
      get => csi ??= new();
      set => csi = value;
    }

    private Common csePersonNotFound;
    private DateWorkArea maxDate;
    private Common commit;
    private Common interstateCaseExists;
    private Common promatchNonCsenet;
    private Common promatchAlreadyReceived;
    private Common promatchCreated;
    private Common csenetCsiRequest;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private Code csi;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonNotFound.
    /// </summary>
    [JsonPropertyName("csePersonNotFound")]
    public Common CsePersonNotFound
    {
      get => csePersonNotFound ??= new();
      set => csePersonNotFound = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of InterstateCaseExists.
    /// </summary>
    [JsonPropertyName("interstateCaseExists")]
    public Common InterstateCaseExists
    {
      get => interstateCaseExists ??= new();
      set => interstateCaseExists = value;
    }

    /// <summary>
    /// A value of PromatchNonCsenet.
    /// </summary>
    [JsonPropertyName("promatchNonCsenet")]
    public Common PromatchNonCsenet
    {
      get => promatchNonCsenet ??= new();
      set => promatchNonCsenet = value;
    }

    /// <summary>
    /// A value of PromatchAlreadyReceived.
    /// </summary>
    [JsonPropertyName("promatchAlreadyReceived")]
    public Common PromatchAlreadyReceived
    {
      get => promatchAlreadyReceived ??= new();
      set => promatchAlreadyReceived = value;
    }

    /// <summary>
    /// A value of PromatchCreated.
    /// </summary>
    [JsonPropertyName("promatchCreated")]
    public Common PromatchCreated
    {
      get => promatchCreated ??= new();
      set => promatchCreated = value;
    }

    /// <summary>
    /// A value of CsiRequest.
    /// </summary>
    [JsonPropertyName("csiRequest")]
    public Common CsiRequest
    {
      get => csiRequest ??= new();
      set => csiRequest = value;
    }

    private Common csePersonNotFound;
    private Common commit;
    private Common interstateCaseExists;
    private Common promatchNonCsenet;
    private Common promatchAlreadyReceived;
    private Common promatchCreated;
    private Common csiRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ConvertDate.
    /// </summary>
    [JsonPropertyName("convertDate")]
    public DateWorkArea ConvertDate
    {
      get => convertDate ??= new();
      set => convertDate = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private InterstateRequest interstateRequest;
    private Infrastructure infrastructure;
    private DateWorkArea convertDate;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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

    private InterstateRequest interstateRequest;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private Code code;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
