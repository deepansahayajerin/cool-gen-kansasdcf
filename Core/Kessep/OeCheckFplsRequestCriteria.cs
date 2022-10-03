// Program: OE_CHECK_FPLS_REQUEST_CRITERIA, ID: 372362496, model: 746.
// Short name: SWE00881
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CHECK_FPLS_REQUEST_CRITERIA.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block checks if an FPLS request needs to be created.
/// </para>
/// </summary>
[Serializable]
public partial class OeCheckFplsRequestCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CHECK_FPLS_REQUEST_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCheckFplsRequestCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCheckFplsRequestCriteria.
  /// </summary>
  public OeCheckFplsRequestCriteria(IContext context, Import import,
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
    // -----------------------------------------------
    //   Date		Developer	Description
    // 00/00/00	Sid	  	Initial Creation
    // 01/13/96  	T.O.Redmond 	Modify Extract Rqts
    // 07/15/96	T.O.Redmond	Remove Task and Plan Task logic
    // -----------------------------------------------
    export.ActionNeeded.ActionEntry = "S";

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
    }
    else
    {
      export.ActionNeeded.ActionEntry = "N";
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadCase())
    {
      export.ActionNeeded.ActionEntry = "N";
      ExitState = "CASE_NF";

      return;
    }

    // ************************************************
    // *Determine if this CASE has been in a Locate   *
    // *status for over 45 days. Federal Requirements *
    // *state that all available resources be used to *
    // *locate an individual within 75 days of the    *
    // *establishment of a CASE.                      *
    // * Since this Job is a monthly run, the check is*
    // *for 45 days, instead of 75 to give time for the*
    // *Job to run
    // 
    // *
    // ************************************************
    // ************************************************
    // Since job frequency is changed from monthly to
    // weekly,the check is changed from 45 days to
    // 68 days, instead of 75 to give time for the Job
    // to run.
    // ************************************************
    if (ReadCaseUnit())
    {
      if (CharAt(entities.ExistingCaseUnit.State, 3) == 'Y')
      {
        export.ActionNeeded.ActionEntry = "N";

        return;
      }
      else if (CharAt(entities.ExistingCaseUnit.State, 3) != 'Y')
      {
        if (Lt(entities.ExistingCaseUnit.CreatedTimestamp,
          entities.ExistingCaseUnit.LastUpdatedTimestamp))
        {
          if (Lt(AddDays(import.ProgramProcessingInfo.ProcessDate, -68),
            Date(entities.ExistingCaseUnit.LastUpdatedTimestamp)))
          {
            export.ActionNeeded.ActionEntry = "N";

            return;
          }
        }
      }
    }

    if (Lt(AddDays(import.ProgramProcessingInfo.ProcessDate, -68),
      entities.ExistingCase.CseOpenDate))
    {
      export.ActionNeeded.ActionEntry = "N";

      return;
    }

    // ************************************************
    // *Determine if there is an previous request for *
    // *this CSE Person and it has been a year since  *
    // *we have mailed a request to FPLS - then mail a*
    // *new request.
    // 
    // *
    // ************************************************
    if (ReadFplsLocateRequest2())
    {
      if (Lt(AddDays(import.ProgramProcessingInfo.ProcessDate, -335),
        entities.ExistingFplsLocateRequest.RequestSentDate))
      {
        export.ActionNeeded.ActionEntry = "N";

        return;
      }
    }

    // ************************************************
    // *Determine if there is an existing request for *
    // *this CSE Person that has not yet been mailed  *
    // *to the FPLS Agency.                           *
    // ************************************************
    if (ReadFplsLocateRequest1())
    {
      export.ActionNeeded.ActionEntry = "N";
    }
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.ExistingCaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", entities.ExistingCsePerson.Number);
        db.SetString(command, "casNo", entities.ExistingCase.Number);
        db.SetDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.State = db.GetString(reader, 1);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 2);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.ExistingCaseUnit.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.ExistingCaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 6);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 7);
        entities.ExistingCaseUnit.Populated = true;
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
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadFplsLocateRequest1()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(
          command, "requestSentDate", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest2()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateRequest.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ActionNeeded.
    /// </summary>
    [JsonPropertyName("actionNeeded")]
    public Common ActionNeeded
    {
      get => actionNeeded ??= new();
      set => actionNeeded = value;
    }

    private FplsLocateRequest fplsLocateRequest;
    private CsePerson csePerson;
    private Common actionNeeded;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public Code Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private DateWorkArea nullDate;
    private Code blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
    }

    private CaseUnit existingCaseUnit;
    private Case1 existingCase;
    private CsePerson existingCsePerson;
    private FplsLocateRequest existingFplsLocateRequest;
  }
#endregion
}
