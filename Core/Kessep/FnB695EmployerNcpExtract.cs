// Program: FN_B695_EMPLOYER_NCP_EXTRACT, ID: 374412048, model: 746.
// Short name: SWEF695B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B695_EMPLOYER_NCP_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB695EmployerNcpExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B695_EMPLOYER_NCP_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB695EmployerNcpExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB695EmployerNcpExtract.
  /// </summary>
  public FnB695EmployerNcpExtract(IContext context, Import import, Export export)
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
    // ***********************************************************
    // Initial Version - 04/25/00 SWETTREM
    // ************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB695Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    foreach(var item in ReadEmployerCsePersonLegalAction())
    {
      if ((local.PrevEmployer.Identifier != entities.Employer.Identifier || !
        Equal(local.PrevCsePerson.Number, entities.CsePerson.Number) || !
        Equal(local.PrevLegalAction.StandardNumber,
        entities.LegalAction.StandardNumber)) && (
          !IsEmpty(local.PrevCsePerson.Number) || local
        .PrevEmployer.Identifier != 0 || !
        IsEmpty(local.PrevLegalAction.StandardNumber)) && AsChar
        (local.Error.Flag) != 'Y')
      {
        // **********************************
        // Print the employer record
        // **********************************
        local.KpcExternalParms.Parm1 = "GR";
        UseFnExtWriteEmployerNcpRecord2();

        if (IsEmpty(local.KpcExternalParms.Parm1))
        {
          ++local.EmployerRecsProcessed.Count;
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_EMP_FILE";

          break;
        }
      }
      else if (Equal(entities.Employer.Name, local.PrevEmployer.Name) && entities
        .Employer.Identifier == local.PrevEmployer.Identifier && Equal
        (entities.CsePerson.Number, local.PrevCsePerson.Number) && Equal
        (entities.LegalAction.StandardNumber,
        local.PrevLegalAction.StandardNumber))
      {
        continue;
      }
      else
      {
        local.Error.Flag = "";
      }

      if (local.PrevEmployer.Identifier != entities.Employer.Identifier)
      {
        if (ReadEmployerAddress())
        {
          local.EmployerNcpRecord.Name = entities.Employer.Name ?? Spaces(50);
          local.EmployerNcpRecord.Street1 =
            entities.EmployerAddress.Street1 ?? Spaces(50);
          local.EmployerNcpRecord.Street2 =
            entities.EmployerAddress.Street2 ?? Spaces(50);
          local.EmployerNcpRecord.Street3 =
            entities.EmployerAddress.Street3 ?? Spaces(50);
          local.EmployerNcpRecord.City = entities.EmployerAddress.City ?? Spaces
            (20);
          local.EmployerNcpRecord.State = entities.EmployerAddress.State ?? Spaces
            (2);
          local.EmployerNcpRecord.ZipCode =
            entities.EmployerAddress.ZipCode ?? Spaces(5);
          local.EmployerNcpRecord.PhoneNumber =
            NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 13, 3)
            + entities.Employer.PhoneNo;
          local.EmployerNcpRecord.Ein = entities.Employer.Ein ?? Spaces(10);
        }
        else
        {
          ExitState = "FN0000_NO_ADDRESS_FOUND";
          UseFnB695PrintErrorLine1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          local.Error.Flag = "Y";
        }
      }

      if (!Equal(local.PrevCsePerson.Number, entities.CsePerson.Number))
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        // ************************************************
        // *Call EAB to retrieve information about a CSE  *
        // *PERSON from the ADABAS system.                *
        // ************************************************
        UseCabReadAdabasPersonBatch();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseFnB695PrintErrorLine1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          local.Error.Flag = "Y";
        }

        local.EmployerNcpRecord.FirstName = local.CsePersonsWorkSet.FirstName;
        local.EmployerNcpRecord.MiddleInitial =
          local.CsePersonsWorkSet.MiddleInitial;
        local.EmployerNcpRecord.LastName = local.CsePersonsWorkSet.LastName;
        local.EmployerNcpRecord.Ssn = local.CsePersonsWorkSet.Ssn;
      }

      if (!Equal(local.PrevLegalAction.StandardNumber,
        entities.LegalAction.StandardNumber))
      {
        // ***********************************
        // Check state court order is from
        // ***********************************
        ReadTribunalFips();

        // ******************************
        // Get Court Order information
        // ******************************
        if (Equal(entities.Fips.StateAbbreviation, "KS"))
        {
          local.EmployerNcpRecord.CountyId =
            Substring(entities.LegalAction.StandardNumber, 1, 2);
          local.Find.Count = Find(entities.LegalAction.StandardNumber, "*");

          if (local.Find.Count == 0)
          {
            local.EmployerNcpRecord.CourtOrderNumber =
              Substring(entities.LegalAction.StandardNumber, 3, 10);
          }
          else
          {
            local.EmployerNcpRecord.CourtOrderNumber =
              Substring(entities.LegalAction.StandardNumber,
              LegalAction.StandardNumber_MaxLength, 3, local.Find.Count - 3) + " " +
              Substring
              (entities.LegalAction.StandardNumber,
              LegalAction.StandardNumber_MaxLength, local.Find.Count + 1, 12 -
              local.Find.Count);
          }
        }
        else
        {
          local.EmployerNcpRecord.CountyId = "";
          local.EmployerNcpRecord.CourtOrderNumber =
            entities.LegalAction.StandardNumber ?? Spaces(12);
        }
      }

      ++local.EmployerRecsRead.Count;
      MoveEmployer(entities.Employer, local.PrevEmployer);
      local.PrevCsePerson.Number = entities.CsePerson.Number;
      local.PrevLegalAction.StandardNumber =
        entities.LegalAction.StandardNumber;
    }

    if (local.EmployerRecsRead.Count > 0)
    {
      // **********************************
      // Print the employer record
      // **********************************
      local.KpcExternalParms.Parm1 = "GR";
      UseFnExtWriteEmployerNcpRecord2();

      if (IsEmpty(local.KpcExternalParms.Parm1))
      {
        ++local.EmployerRecsProcessed.Count;
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_EMP_FILE";
        UseFnB695PrintErrorLine1();
      }
    }

    // ****************************
    // CLOSE OUTPUT FILE
    // ****************************
    local.KpcExternalParms.Parm1 = "CF";
    UseFnExtWriteEmployerNcpRecord1();
    local.CloseInd.Flag = "Y";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Close the Error Report.
      UseFnB695PrintErrorLine2();
      UseFnB695PrintControl();
    }
    else
    {
      // : Report the error that occurred and close the Error Report.
      //   ABEND the procedure once reporting is complete.
      UseFnB695PrintErrorLine2();
      UseFnB695PrintControl();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Close.Number = "CLOSE";
    UseEabReadCsePersonBatch();
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
    local.Ae.Flag = useExport.Ae.Flag;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Close.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseFnB695Housekeeping()
  {
    var useImport = new FnB695Housekeeping.Import();
    var useExport = new FnB695Housekeeping.Export();

    Call(FnB695Housekeeping.Execute, useImport, useExport);

    local.CurrentRun.Date = useExport.CurrentRun.Date;
  }

  private void UseFnB695PrintControl()
  {
    var useImport = new FnB695PrintControl.Import();
    var useExport = new FnB695PrintControl.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.EmployerRecsRead.Count = local.EmployerRecsRead.Count;
    useImport.EmploerRecsProcessed.Count = local.EmployerRecsProcessed.Count;

    Call(FnB695PrintControl.Execute, useImport, useExport);
  }

  private void UseFnB695PrintErrorLine1()
  {
    var useImport = new FnB695PrintErrorLine.Import();
    var useExport = new FnB695PrintErrorLine.Export();

    useImport.Employer.Assign(entities.Employer);
    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(FnB695PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB695PrintErrorLine2()
  {
    var useImport = new FnB695PrintErrorLine.Import();
    var useExport = new FnB695PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB695PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnExtWriteEmployerNcpRecord1()
  {
    var useImport = new FnExtWriteEmployerNcpRecord.Import();
    var useExport = new FnExtWriteEmployerNcpRecord.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteEmployerNcpRecord.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private void UseFnExtWriteEmployerNcpRecord2()
  {
    var useImport = new FnExtWriteEmployerNcpRecord.Import();
    var useExport = new FnExtWriteEmployerNcpRecord.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    useImport.EmployerNcpRecord.Assign(local.EmployerNcpRecord);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteEmployerNcpRecord.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.State = db.GetNullableString(reader, 7);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 9);
        entities.EmployerAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEmployerCsePersonLegalAction()
  {
    entities.Employer.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadEmployerCsePersonLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.CurrentRun.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.LegalAction.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.Classification = db.GetString(reader, 8);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 10);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 11);
        entities.Employer.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadTribunalFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Fips.Location = db.GetInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Fips.County = db.GetInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Fips.State = db.GetInt32(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of CurrentRun.
    /// </summary>
    [JsonPropertyName("currentRun")]
    public DateWorkArea CurrentRun
    {
      get => currentRun ??= new();
      set => currentRun = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of EmployerRecsRead.
    /// </summary>
    [JsonPropertyName("employerRecsRead")]
    public Common EmployerRecsRead
    {
      get => employerRecsRead ??= new();
      set => employerRecsRead = value;
    }

    /// <summary>
    /// A value of EmployerRecsProcessed.
    /// </summary>
    [JsonPropertyName("employerRecsProcessed")]
    public Common EmployerRecsProcessed
    {
      get => employerRecsProcessed ??= new();
      set => employerRecsProcessed = value;
    }

    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of EmployerNcpRecord.
    /// </summary>
    [JsonPropertyName("employerNcpRecord")]
    public EmployerNcpRecord EmployerNcpRecord
    {
      get => employerNcpRecord ??= new();
      set => employerNcpRecord = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePersonsWorkSet Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of Find.
    /// </summary>
    [JsonPropertyName("find")]
    public Common Find
    {
      get => find ??= new();
      set => find = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of PrevEmployer.
    /// </summary>
    [JsonPropertyName("prevEmployer")]
    public Employer PrevEmployer
    {
      get => prevEmployer ??= new();
      set => prevEmployer = value;
    }

    /// <summary>
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
    }

    /// <summary>
    /// A value of PrevLegalAction.
    /// </summary>
    [JsonPropertyName("prevLegalAction")]
    public LegalAction PrevLegalAction
    {
      get => prevLegalAction ??= new();
      set => prevLegalAction = value;
    }

    private Common error;
    private DateWorkArea currentRun;
    private Common closeInd;
    private Common employerRecsRead;
    private Common employerRecsProcessed;
    private KpcExternalParms kpcExternalParms;
    private EmployerNcpRecord employerNcpRecord;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet close;
    private Common ae;
    private Common find;
    private Common length;
    private Common temp;
    private Employer prevEmployer;
    private CsePerson prevCsePerson;
    private LegalAction prevLegalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Employer employer;
    private EmployerAddress employerAddress;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private Tribunal tribunal;
    private Fips fips;
  }
#endregion
}
