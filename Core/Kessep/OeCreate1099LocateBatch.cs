// Program: OE_CREATE_1099_LOCATE_BATCH, ID: 371801927, model: 746.
// Short name: SWE01538
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CREATE_1099_LOCATE_BATCH.
/// </para>
/// <para>
/// RESP: OBLGEST
/// This elementary process CREATES a 1099_LOCATE_REQUEST.
/// Use for batch Processing only.
/// </para>
/// </summary>
[Serializable]
public partial class OeCreate1099LocateBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_1099_LOCATE_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreate1099LocateBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreate1099LocateBatch.
  /// </summary>
  public OeCreate1099LocateBatch(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	   DESCRIPTION
    // unknown   	MM/DD/YY   Initial Code
    // T.O.Redmond	04/17/96   Add logic to disallow a create if a prior request 
    // was sent within the last 100 days or if there is no Social Security
    // Number.
    // T.O.Redmond	05/27/96   Use Batch Si Read
    // T.O.Redmond	07/15/96   Move 041796 logic to the Prad.
    // R. Welborn      09/09/96   IDCR 177 Leg Act Classification
    //                            change O to J.
    // ******** END MAINTENANCE LOG ****************
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    UseOeDetermineAdcPgmParticipatn();

    if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
    {
      // IDCR# 39943 -
      // Default Case Programs to ADC, if the Case Program does not exist.
      ExitState = "ACO_NN0000_ALL_OK";
      local.Temp.AfdcCode = "A";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.CaseInAdcProgram.SelectChar) == 'Y')
    {
      local.Temp.AfdcCode = "A";
    }
    else
    {
      local.Temp.AfdcCode = "N";
    }

    Read1099LocateRequest();
    local.Next.Identifier = entities.ExistingLast.Identifier + 1;

    // ************************************************
    // *We are joining the CSE Number and the         *
    // *identifier into case-id so that this will be  *
    // *available for us upon return from the IRS     *
    // ************************************************
    local.WorkArea.Text3 = NumberToString(local.Next.Identifier, 3);
    local.Temp.CaseIdNo = entities.ExistingCsePerson.Number + local
      .WorkArea.Text3;

    // ************************************************
    // *Read Legal Action Person and Legal Action to  *
    // *determine if there is a legal action and if so*
    // *make a determination if the classification    *
    // *states that this is an Order.                 *
    // ************************************************
    local.Temp.CourtOrAdminOrdInd = "N";

    if (ReadLegalActionPersonLegalAction())
    {
      if (AsChar(entities.ExistingLegalAction.Classification) == 'O' || AsChar
        (entities.ExistingLegalAction.Classification) == 'J')
      {
        local.Temp.CourtOrAdminOrdInd = "Y";
      }
      else
      {
        local.Temp.CourtOrAdminOrdInd = "N";
      }
    }
    else if (ReadLegalActionPersonLegalActionLegalActionDetail())
    {
      if (AsChar(entities.ExistingLegalAction.Classification) == 'O' || AsChar
        (entities.ExistingLegalAction.Classification) == 'J')
      {
        local.Temp.CourtOrAdminOrdInd = "Y";
      }
      else
      {
        local.Temp.CourtOrAdminOrdInd = "N";
      }
    }
    else
    {
      local.Temp.CourtOrAdminOrdInd = "N";
    }

    try
    {
      Create1099LocateRequest();
      export.Data1099LocateRequest.Assign(entities.New1);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_CONTENTION_IN_GENKEY";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "1099_LOCATE_REQUEST_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseOeDetermineAdcPgmParticipatn()
  {
    var useImport = new OeDetermineAdcPgmParticipatn.Import();
    var useExport = new OeDetermineAdcPgmParticipatn.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Ap.Number = entities.ExistingCsePerson.Number;

    Call(OeDetermineAdcPgmParticipatn.Execute, useImport, useExport);

    local.CaseInAdcProgram.SelectChar = useExport.CaseInAdcProgram.SelectChar;
  }

  private void Create1099LocateRequest()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var identifier = local.Next.Identifier;
    var ssn = import.Data1099LocateRequest.Ssn;
    var localCode = "000";
    var lastName = import.Data1099LocateRequest.LastName ?? "";
    var afdcCode = local.Temp.AfdcCode ?? "";
    var caseIdNo = local.Temp.CaseIdNo ?? "";
    var courtOrAdminOrdInd = local.Temp.CourtOrAdminOrdInd ?? "";
    var createdBy = global.TranCode;
    var createdTimestamp = Now();
    var firstName = import.Data1099LocateRequest.FirstName ?? "";
    var requestSentDate = local.NullDate.Date;
    var middleInitial = import.Data1099LocateRequest.MiddleInitial ?? "";

    entities.New1.Populated = false;
    Update("Create1099LocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "ssn", ssn);
        db.SetNullableString(command, "localCode", localCode);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "afdcCode", afdcCode);
        db.SetNullableString(command, "caseIdNo", caseIdNo);
        db.SetNullableString(command, "ctOrAdmOrdInd", courtOrAdminOrdInd);
        db.SetNullableString(command, "noMatchCode", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "middleInitial", middleInitial);
      });

    entities.New1.CspNumber = cspNumber;
    entities.New1.Identifier = identifier;
    entities.New1.Ssn = ssn;
    entities.New1.LocalCode = localCode;
    entities.New1.LastName = lastName;
    entities.New1.AfdcCode = afdcCode;
    entities.New1.CaseIdNo = caseIdNo;
    entities.New1.CourtOrAdminOrdInd = courtOrAdminOrdInd;
    entities.New1.NoMatchCode = "";
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTimestamp = createdTimestamp;
    entities.New1.FirstName = firstName;
    entities.New1.RequestSentDate = requestSentDate;
    entities.New1.MiddleInitial = middleInitial;
    entities.New1.Populated = true;
  }

  private bool Read1099LocateRequest()
  {
    entities.ExistingLast.Populated = false;

    return Read("Read1099LocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLast.CspNumber = db.GetString(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.RequestSentDate = db.GetNullableDate(reader, 2);
        entities.ExistingLast.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadLegalActionPersonLegalAction()
  {
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionPersonLegalAction",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 2);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 4);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPerson.EndReason =
          db.GetNullableString(reader, 6);
        entities.ExistingLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.ExistingLegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 12);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 13);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 14);
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPersonLegalActionLegalActionDetail()
  {
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.Exisitng.Populated = false;

    return Read("ReadLegalActionPersonLegalActionLegalActionDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 4);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPerson.EndReason =
          db.GetNullableString(reader, 6);
        entities.ExistingLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.ExistingLegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.Exisitng.LgaIdentifier = db.GetInt32(reader, 9);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 10);
        entities.Exisitng.Number = db.GetInt32(reader, 10);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 12);
        entities.Exisitng.LgaIdentifier = db.GetInt32(reader, 12);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 13);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 14);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 15);
        entities.Exisitng.EndDate = db.GetNullableDate(reader, 16);
        entities.Exisitng.EffectiveDate = db.GetDate(reader, 17);
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.Exisitng.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private Data1099LocateRequest data1099LocateRequest;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    private Data1099LocateRequest data1099LocateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Data1099LocateRequest Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of NullSsn.
    /// </summary>
    [JsonPropertyName("nullSsn")]
    public Data1099LocateRequest NullSsn
    {
      get => nullSsn ??= new();
      set => nullSsn = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Data1099LocateRequest Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of CaseInAdcProgram.
    /// </summary>
    [JsonPropertyName("caseInAdcProgram")]
    public Common CaseInAdcProgram
    {
      get => caseInAdcProgram ??= new();
      set => caseInAdcProgram = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private WorkArea workArea;
    private Data1099LocateRequest next;
    private Data1099LocateRequest nullSsn;
    private DateWorkArea nullDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Data1099LocateRequest temp;
    private Common caseInAdcProgram;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingLegalActionPerson")]
    public LegalActionPerson ExistingLegalActionPerson
    {
      get => existingLegalActionPerson ??= new();
      set => existingLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Data1099LocateRequest ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Data1099LocateRequest New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Exisitng.
    /// </summary>
    [JsonPropertyName("exisitng")]
    public LegalActionDetail Exisitng
    {
      get => exisitng ??= new();
      set => exisitng = value;
    }

    private LegalActionPerson existingLegalActionPerson;
    private LegalAction existingLegalAction;
    private Data1099LocateRequest existingLast;
    private CsePerson existingCsePerson;
    private Data1099LocateRequest new1;
    private LegalActionDetail exisitng;
  }
#endregion
}
