// Program: CREATE_1099_LOCATE, ID: 372359635, model: 746.
// Short name: SWE00169
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_1099_LOCATE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This elementary process  CREATES a 1099_LOCATE_REQUEST.
/// </para>
/// </summary>
[Serializable]
public partial class Create1099Locate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_1099_LOCATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Create1099Locate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Create1099Locate.
  /// </summary>
  public Create1099Locate(IContext context, Import import, Export export):
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
    // T.O.Redmond	04/17/96   Add logic to
    // disallow a create if a prior request was sent
    // within the last 100 days or if there is no
    // Social Security Number.
    // MK              09/1998
    // Add EXPORT CSE PERSON WORKSET VIEW.
    // Add MOVE CSE LOCAL TO EXPORT to allow person name and ssn to be sent to 
    // screen.
    // ******** END MAINTENANCE LOG ****************
    // Please note that any changes to this Prad should also be made in the 
    // Batch version. Please do not remove this note from this prad.
    local.Current.Date = Now().Date;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseSiReadCsePerson();

    // *********************************
    // MK              09/98
    // Add EXPORT CSE PERSON WORKSET VIEW.
    // Add MOVE CSE LOCAL TO EXPORT to allow person name and ssn to be sent to 
    // screen.
    // **********************************
    export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
      (local.CsePersonsWorkSet.Ssn, "000000000"))
    {
      export.CsePersonsWorkSet.Ssn = "";
      ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

      return;
    }

    UseOeDetermineAdcPgmParticipatn();

    if (AsChar(local.CaseInAdcProgram.SelectChar) == 'Y')
    {
      local.Temp.AfdcCode = "A";
    }
    else
    {
      local.Temp.AfdcCode = "N";
    }

    if (Read1099LocateRequest())
    {
      // ************************************************
      // * Please note that as of 04/16/1996 it was     *
      // * decided that a request could not be created  *
      // * if a prior request had been send  within the *
      // * past 3 months.
      // 
      // *
      // ************************************************
      if (Equal(entities.ExistingLast.RequestSentDate, local.NullDate.Date))
      {
        ExitState = "CANNOT_ADD_AN_EXISTING_OCCURRENC";

        return;
      }

      if (Lt(AddDays(local.Current.Date, -100),
        entities.ExistingLast.RequestSentDate))
      {
        ExitState = "OE0000_REQUEST_IS_WITHIN_3_MNTHS";

        return;
      }
    }

    // ************************************************
    // *We are joining the CSE Number and the         *
    // *identifier into case-id so that this will be  *
    // *available for us upon return from the IRS     *
    // ************************************************
    local.Temp.CaseIdNo = entities.ExistingCsePerson.Number + NumberToString
      (entities.ExistingLast.Identifier + 1, 15);

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
    else if (ReadLegalActionDetailLegalActionLegalActionPerson())
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

    useImport.Ap.Number = entities.ExistingCsePerson.Number;

    Call(OeDetermineAdcPgmParticipatn.Execute, useImport, useExport);

    local.CaseInAdcProgram.SelectChar = useExport.CaseInAdcProgram.SelectChar;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void Create1099LocateRequest()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var identifier = entities.ExistingLast.Identifier + 1;
    var ssn = local.CsePersonsWorkSet.Ssn;
    var localCode = "000";
    var lastName = local.CsePersonsWorkSet.LastName;
    var afdcCode = local.Temp.AfdcCode ?? "";
    var caseIdNo = local.Temp.CaseIdNo ?? "";
    var courtOrAdminOrdInd = local.Temp.CourtOrAdminOrdInd ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var firstName = local.CsePersonsWorkSet.FirstName;

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
        db.SetNullableDate(command, "requestSentDate", null);
        db.SetNullableString(command, "middleInitial", "");
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
    entities.New1.RequestSentDate = null;
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

  private bool ReadLegalActionDetailLegalActionLegalActionPerson()
  {
    entities.Exisitng.Populated = false;
    entities.ExistingLegalActionPerson.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionDetailLegalActionLegalActionPerson",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Exisitng.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.Exisitng.Number = db.GetInt32(reader, 1);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 1);
        entities.Exisitng.EndDate = db.GetNullableDate(reader, 2);
        entities.Exisitng.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 4);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 5);
        entities.ExistingLegalAction.EndDate = db.GetNullableDate(reader, 6);
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 10);
        entities.ExistingLegalActionPerson.Role = db.GetString(reader, 11);
        entities.ExistingLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingLegalActionPerson.EndReason =
          db.GetNullableString(reader, 13);
        entities.ExistingLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 14);
        entities.ExistingLegalActionPerson.CreatedBy = db.GetString(reader, 15);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 16);
        entities.Exisitng.Populated = true;
        entities.ExistingLegalActionPerson.Populated = true;
        entities.ExistingLegalAction.Populated = true;
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
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Data1099LocateRequest data1099LocateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private DateWorkArea current;
    private DateWorkArea nullDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Data1099LocateRequest temp;
    private Common caseInAdcProgram;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Exisitng.
    /// </summary>
    [JsonPropertyName("exisitng")]
    public LegalActionDetail Exisitng
    {
      get => exisitng ??= new();
      set => exisitng = value;
    }

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

    private LegalActionDetail exisitng;
    private LegalActionPerson existingLegalActionPerson;
    private LegalAction existingLegalAction;
    private Data1099LocateRequest existingLast;
    private CsePerson existingCsePerson;
    private Data1099LocateRequest new1;
  }
#endregion
}
