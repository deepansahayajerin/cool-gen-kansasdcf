// Program: OE_B413_LOAD_PROACTIVE_MATCH, ID: 373414867, model: 746.
// Short name: SWE01964
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B413_LOAD_PROACTIVE_MATCH.
/// </summary>
[Serializable]
public partial class OeB413LoadProactiveMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B413_LOAD_PROACTIVE_MATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB413LoadProactiveMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB413LoadProactiveMatch.
  /// </summary>
  public OeB413LoadProactiveMatch(IContext context, Import import, Export export)
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
    export.CreatedSuccessfully.Flag = "N";
    export.Commit.Count = import.Commit.Count;
    export.PromatchAlreadyReceived.Count = import.PromatchAlreadyReceived.Count;
    export.PromatchCreated.Count = import.PromatchCreated.Count;

    // ************************************************************
    // Do we already have this record?
    // Only compare the latest record we received for this person,
    // for this other state's case.
    // ************************************************************
    foreach(var item in ReadFcrProactiveMatchResponse2())
    {
      // **********************************************************************
      // Match everything except submitted case id, date received, user field,
      // and identifier.
      // **********************************************************************
      if (Equal(entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode,
        import.FcrProactiveMatchResponse.TransmitterStateOrTerrCode) && Equal
        (entities.FcrProactiveMatchResponse.MatchedCaseId,
        import.FcrProactiveMatchResponse.MatchedCaseId) && Equal
        (entities.FcrProactiveMatchResponse.MatchedMemberId,
        import.FcrProactiveMatchResponse.MatchedMemberId))
      {
      }
      else
      {
        // *****************************************************
        // The person may be on more than one interstate case,
        // either in the same state or in different states.
        // Keep searching.
        // *****************************************************
        continue;
      }

      if (Equal(entities.FcrProactiveMatchResponse.ResponseCode,
        import.FcrProactiveMatchResponse.ResponseCode) && AsChar
        (entities.FcrProactiveMatchResponse.MatchedCaseType) == AsChar
        (import.FcrProactiveMatchResponse.MatchedCaseType) && Equal
        (entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate,
        import.FcrProactiveMatchResponse.MatchedFcrCaseRegDate) && AsChar
        (entities.FcrProactiveMatchResponse.MatchedCaseOrderInd) == AsChar
        (import.FcrProactiveMatchResponse.MatchedCaseOrderInd) && Equal
        (entities.FcrProactiveMatchResponse.MatchedParticipantType,
        import.FcrProactiveMatchResponse.MatchedParticipantType) && AsChar
        (entities.FcrProactiveMatchResponse.ActionTypeCode) == AsChar
        (import.FcrProactiveMatchResponse.ActionTypeCode))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.LastName,
        import.FcrProactiveMatchResponse.LastName) && Equal
        (entities.FcrProactiveMatchResponse.FirstName,
        import.FcrProactiveMatchResponse.FirstName) && Equal
        (entities.FcrProactiveMatchResponse.MiddleName,
        import.FcrProactiveMatchResponse.MiddleName) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonDod,
        import.FcrProactiveMatchResponse.MatchedPersonDod) && Equal
        (entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn,
        import.FcrProactiveMatchResponse.SubmittedOrMatchedSsn))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.AssociatedDob1,
        import.FcrProactiveMatchResponse.AssociatedDob1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedDod1,
        import.FcrProactiveMatchResponse.AssociatedDod1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedFirstName1,
        import.FcrProactiveMatchResponse.AssociatedFirstName1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedLastName1,
        import.FcrProactiveMatchResponse.AssociatedLastName1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedMiddleName1,
        import.FcrProactiveMatchResponse.AssociatedMiddleName1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1,
        import.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedParticipantType1,
        import.FcrProactiveMatchResponse.AssociatedParticipantType1) && AsChar
        (entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1) == AsChar
        (import.FcrProactiveMatchResponse.AssociatedPersonSexCode1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedSsn1,
        import.FcrProactiveMatchResponse.AssociatedSsn1) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedStateMembId1,
        import.FcrProactiveMatchResponse.AssociatedStateMembId1))
      {
      }
      else if (Equal(entities.FcrProactiveMatchResponse.ResponseCode, "MM"))
      {
        continue;
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.AssociatedDob2,
        import.FcrProactiveMatchResponse.AssociatedDob2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedDod2,
        import.FcrProactiveMatchResponse.AssociatedDod2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedFirstName2,
        import.FcrProactiveMatchResponse.AssociatedFirstName2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedLastName2,
        import.FcrProactiveMatchResponse.AssociatedLastName2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedMiddleName2,
        import.FcrProactiveMatchResponse.AssociatedMiddleName2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2,
        import.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedParticipantType2,
        import.FcrProactiveMatchResponse.AssociatedParticipantType2) && AsChar
        (entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2) == AsChar
        (import.FcrProactiveMatchResponse.AssociatedPersonSexCode2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedSsn2,
        import.FcrProactiveMatchResponse.AssociatedSsn2) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedStateMembId2,
        import.FcrProactiveMatchResponse.AssociatedStateMembId2))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.AssociatedDob3,
        import.FcrProactiveMatchResponse.AssociatedDob3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedDod3,
        import.FcrProactiveMatchResponse.AssociatedDod3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedFirstName3,
        import.FcrProactiveMatchResponse.AssociatedFirstName3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedLastName3,
        import.FcrProactiveMatchResponse.AssociatedLastName3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedMiddleName3,
        import.FcrProactiveMatchResponse.AssociatedMiddleName3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3,
        import.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedParticipantType3,
        import.FcrProactiveMatchResponse.AssociatedParticipantType3) && AsChar
        (entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3) == AsChar
        (import.FcrProactiveMatchResponse.AssociatedPersonSexCode3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedSsn3,
        import.FcrProactiveMatchResponse.AssociatedSsn3) && Equal
        (entities.FcrProactiveMatchResponse.AssociatedStateMembId3,
        import.FcrProactiveMatchResponse.AssociatedStateMembId3))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1,
        import.FcrProactiveMatchResponse.MatchedPersonAddFirstName1) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1,
        import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1,
        import.FcrProactiveMatchResponse.MatchedPersonAddLastName1))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2,
        import.FcrProactiveMatchResponse.MatchedPersonAddFirstName2) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2,
        import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2,
        import.FcrProactiveMatchResponse.MatchedPersonAddLastName2))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3,
        import.FcrProactiveMatchResponse.MatchedPersonAddFirstName3) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3,
        import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3,
        import.FcrProactiveMatchResponse.MatchedPersonAddLastName3))
      {
      }
      else
      {
        break;
      }

      if (Equal(entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4,
        import.FcrProactiveMatchResponse.MatchedPersonAddFirstName4) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4,
        import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4) && Equal
        (entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4,
        import.FcrProactiveMatchResponse.MatchedPersonAddLastName4))
      {
      }
      else
      {
        break;
      }

      // *******************************
      // Exact duplicate found
      // *******************************
      ++export.PromatchAlreadyReceived.Count;

      return;
    }

    local.FcrProactiveMatchResponse.Assign(import.FcrProactiveMatchResponse);
    local.FcrProactiveMatchResponse.Identifier = 1;
    ReadFcrProactiveMatchResponse1();
    ++local.FcrProactiveMatchResponse.Identifier;
    UseOeCabCreateProactiveMatch();

    if (IsExitState("FCR_PROACTIVE_MATCH_RESPONSE_AE"))
    {
      local.EabReportSend.RptDetail =
        "Create FCR Proactive Match Response already exists for person: " + (
          local.FcrProactiveMatchResponse.StateMemberId ?? "");
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
    else if (IsExitState("FCR_PROACTIVE_MATCH_RESPONSE_PV"))
    {
      local.EabReportSend.RptDetail =
        "Create FCR Proactive Match response permitted value violation for person: " +
        (local.FcrProactiveMatchResponse.StateMemberId ?? "");
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
    else
    {
      export.CreatedSuccessfully.Flag = "Y";
      ++export.PromatchCreated.Count;
      ++export.Commit.Count;
    }
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

  private void UseOeCabCreateProactiveMatch()
  {
    var useImport = new OeCabCreateProactiveMatch.Import();
    var useExport = new OeCabCreateProactiveMatch.Export();

    useImport.FcrProactiveMatchResponse.Assign(local.FcrProactiveMatchResponse);

    Call(OeCabCreateProactiveMatch.Execute, useImport, useExport);

    local.FcrProactiveMatchResponse.Assign(useExport.FcrProactiveMatchResponse);
  }

  private bool ReadFcrProactiveMatchResponse1()
  {
    local.FcrProactiveMatchResponse.Populated = false;

    return Read("ReadFcrProactiveMatchResponse1",
      null,
      (db, reader) =>
      {
        local.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 0);
        local.FcrProactiveMatchResponse.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFcrProactiveMatchResponse2()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return ReadEach("ReadFcrProactiveMatchResponse2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "stateMemberId",
          import.FcrProactiveMatchResponse.StateMemberId ?? "");
      },
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.ActionTypeCode =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
          db.GetNullableString(reader, 1);
        entities.FcrProactiveMatchResponse.UserField =
          db.GetNullableString(reader, 2);
        entities.FcrProactiveMatchResponse.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.FcrProactiveMatchResponse.FirstName =
          db.GetNullableString(reader, 4);
        entities.FcrProactiveMatchResponse.MiddleName =
          db.GetNullableString(reader, 5);
        entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
          db.GetNullableString(reader, 6);
        entities.FcrProactiveMatchResponse.StateMemberId =
          db.GetNullableString(reader, 7);
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 8);
        entities.FcrProactiveMatchResponse.ResponseCode =
          db.GetNullableString(reader, 9);
        entities.FcrProactiveMatchResponse.MatchedCaseId =
          db.GetNullableString(reader, 10);
        entities.FcrProactiveMatchResponse.MatchedCaseType =
          db.GetNullableString(reader, 11);
        entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
          db.GetNullableString(reader, 12);
        entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
          db.GetNullableDate(reader, 13);
        entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
          db.GetNullableString(reader, 14);
        entities.FcrProactiveMatchResponse.MatchedParticipantType =
          db.GetNullableString(reader, 15);
        entities.FcrProactiveMatchResponse.MatchedMemberId =
          db.GetNullableString(reader, 16);
        entities.FcrProactiveMatchResponse.MatchedPersonDod =
          db.GetNullableDate(reader, 17);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
          db.GetNullableString(reader, 18);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
          db.GetNullableString(reader, 19);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
          db.GetNullableString(reader, 20);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
          db.GetNullableString(reader, 21);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
          db.GetNullableString(reader, 22);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
          db.GetNullableString(reader, 23);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
          db.GetNullableString(reader, 24);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
          db.GetNullableString(reader, 25);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
          db.GetNullableString(reader, 26);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
          db.GetNullableString(reader, 27);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
          db.GetNullableString(reader, 28);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
          db.GetNullableString(reader, 29);
        entities.FcrProactiveMatchResponse.AssociatedSsn1 =
          db.GetNullableString(reader, 30);
        entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
          db.GetNullableString(reader, 31);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
          db.GetNullableString(reader, 32);
        entities.FcrProactiveMatchResponse.AssociatedLastName1 =
          db.GetNullableString(reader, 33);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
          db.GetNullableString(reader, 34);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
          db.GetNullableString(reader, 35);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
          db.GetNullableString(reader, 36);
        entities.FcrProactiveMatchResponse.AssociatedDob1 =
          db.GetNullableDate(reader, 37);
        entities.FcrProactiveMatchResponse.AssociatedDod1 =
          db.GetNullableDate(reader, 38);
        entities.FcrProactiveMatchResponse.AssociatedSsn2 =
          db.GetNullableString(reader, 39);
        entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
          db.GetNullableString(reader, 40);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
          db.GetNullableString(reader, 41);
        entities.FcrProactiveMatchResponse.AssociatedLastName2 =
          db.GetNullableString(reader, 42);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
          db.GetNullableString(reader, 43);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
          db.GetNullableString(reader, 44);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
          db.GetNullableString(reader, 45);
        entities.FcrProactiveMatchResponse.AssociatedDob2 =
          db.GetNullableDate(reader, 46);
        entities.FcrProactiveMatchResponse.AssociatedDod2 =
          db.GetNullableDate(reader, 47);
        entities.FcrProactiveMatchResponse.AssociatedSsn3 =
          db.GetNullableString(reader, 48);
        entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
          db.GetNullableString(reader, 49);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
          db.GetNullableString(reader, 50);
        entities.FcrProactiveMatchResponse.AssociatedLastName3 =
          db.GetNullableString(reader, 51);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
          db.GetNullableString(reader, 52);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
          db.GetNullableString(reader, 53);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
          db.GetNullableString(reader, 54);
        entities.FcrProactiveMatchResponse.AssociatedDob3 =
          db.GetNullableDate(reader, 55);
        entities.FcrProactiveMatchResponse.AssociatedDod3 =
          db.GetNullableDate(reader, 56);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
          db.GetNullableString(reader, 57);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
          db.GetNullableString(reader, 58);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
          db.GetNullableString(reader, 59);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 60);
        entities.FcrProactiveMatchResponse.DateReceived =
          db.GetNullableDate(reader, 61);
        entities.FcrProactiveMatchResponse.LastName =
          db.GetNullableString(reader, 62);
        entities.FcrProactiveMatchResponse.Populated = true;

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
    /// A value of PromatchCreated.
    /// </summary>
    [JsonPropertyName("promatchCreated")]
    public Common PromatchCreated
    {
      get => promatchCreated ??= new();
      set => promatchCreated = value;
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
    /// A value of PromatchAlreadyReceived.
    /// </summary>
    [JsonPropertyName("promatchAlreadyReceived")]
    public Common PromatchAlreadyReceived
    {
      get => promatchAlreadyReceived ??= new();
      set => promatchAlreadyReceived = value;
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
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private Common promatchCreated;
    private ProgramProcessingInfo programProcessingInfo;
    private Common promatchAlreadyReceived;
    private Common commit;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CreatedSuccessfully.
    /// </summary>
    [JsonPropertyName("createdSuccessfully")]
    public Common CreatedSuccessfully
    {
      get => createdSuccessfully ??= new();
      set => createdSuccessfully = value;
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
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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

    private Common createdSuccessfully;
    private Common promatchCreated;
    private Common commit;
    private Common promatchAlreadyReceived;
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
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }
#endregion
}
