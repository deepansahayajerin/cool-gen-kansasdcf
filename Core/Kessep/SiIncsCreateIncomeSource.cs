// Program: SI_INCS_CREATE_INCOME_SOURCE, ID: 371763119, model: 746.
// Short name: SWE01132
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INCS_CREATE_INCOME_SOURCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD adds details about a CSE Person's income source.
/// </para>
/// </summary>
[Serializable]
public partial class SiIncsCreateIncomeSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_CREATE_INCOME_SOURCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsCreateIncomeSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsCreateIncomeSource.
  /// </summary>
  public SiIncsCreateIncomeSource(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date	  Developer		Description
    // 02/26/95  Helen Sharland	Initial Development
    // 09/10/96  G. Lofton		Unemployment ind moved from
    // 				Income Source to Cse Person.
    // ------------------------------------------------------------
    // ---------------------------------------------
    // This PAD creates an income source for a CSE Person.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    // Read the CSE Person that the income source is for
    // ---------------------------------------------
    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!Lt(local.Blank.Date, import.IncomeSource.EndDt))
    {
      import.IncomeSource.EndDt = UseCabSetMaximumDiscontinueDate1();
    }

    try
    {
      CreateIncomeSource();
      import.IncomeSource.Assign(entities.IncomeSource);
      local.DateWorkArea.Date = import.IncomeSource.EndDt;
      import.IncomeSource.EndDt = UseCabSetMaximumDiscontinueDate2();
      ExitState = "ACO_NN0000_ALL_OK";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INCOME_SOURCE_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "INCOME_SOURCE_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (AsChar(entities.CsePerson.FederalInd) != AsChar
      (import.CsePerson.FederalInd))
    {
      try
      {
        UpdateCsePerson();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSE_PERSON_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSE_PERSON_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateIncomeSource()
  {
    var identifier = Now();
    var type1 = import.IncomeSource.Type1;
    var lastQtrIncome = import.IncomeSource.LastQtrIncome.GetValueOrDefault();
    var lastQtr = import.IncomeSource.LastQtr ?? "";
    var lastQtrYr = import.IncomeSource.LastQtrYr.GetValueOrDefault();
    var attribute2NdQtrIncome =
      import.IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault();
    var attribute2NdQtr = import.IncomeSource.Attribute2NdQtr ?? "";
    var attribute2NdQtrYr =
      import.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault();
    var attribute3RdQtrIncome =
      import.IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault();
    var attribute3RdQtr = import.IncomeSource.Attribute3RdQtr ?? "";
    var attribute3RdQtrYr =
      import.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault();
    var attribute4ThQtrIncome =
      import.IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault();
    var attribute4ThQtr = import.IncomeSource.Attribute4ThQtr ?? "";
    var attribute4ThQtrYr =
      import.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault();
    var sentDt = import.IncomeSource.SentDt;
    var returnDt = import.IncomeSource.ReturnDt;
    var returnCd = import.IncomeSource.ReturnCd ?? "";
    var name = import.IncomeSource.Name ?? "";
    var lastUpdatedBy = global.UserId;
    var code = import.IncomeSource.Code ?? "";
    var cspINumber = entities.CsePerson.Number;
    var sendTo = import.IncomeSource.SendTo ?? "";
    var workerId = import.IncomeSource.WorkerId ?? "";
    var startDt = import.IncomeSource.StartDt;
    var endDt = import.IncomeSource.EndDt;
    var note = import.IncomeSource.Note ?? "";

    CheckValid<IncomeSource>("Type1", type1);
    CheckValid<IncomeSource>("SendTo", sendTo);
    entities.IncomeSource.Populated = false;
    Update("CreateIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableDecimal(command, "lastQtrIncome", lastQtrIncome);
        db.SetNullableString(command, "lastQtr", lastQtr);
        db.SetNullableInt32(command, "lastQtrYr", lastQtrYr);
        db.
          SetNullableDecimal(command, "secondQtrIncome", attribute2NdQtrIncome);
          
        db.SetNullableString(command, "secondQtr", attribute2NdQtr);
        db.SetNullableInt32(command, "secondQtrYr", attribute2NdQtrYr);
        db.SetNullableDecimal(command, "thirdQtrIncome", attribute3RdQtrIncome);
        db.SetNullableString(command, "thirdQtr", attribute3RdQtr);
        db.SetNullableInt32(command, "thirdQtrYr", attribute3RdQtrYr);
        db.
          SetNullableDecimal(command, "fourthQtrIncome", attribute4ThQtrIncome);
          
        db.SetNullableString(command, "fourthQtr", attribute4ThQtr);
        db.SetNullableInt32(command, "fourthQtrYr", attribute4ThQtrYr);
        db.SetNullableDate(command, "sentDt", sentDt);
        db.SetNullableDate(command, "returnDt", returnDt);
        db.SetNullableString(command, "returnCd", returnCd);
        db.SetNullableString(command, "name", name);
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", identifier);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "code", code);
        db.SetString(command, "cspINumber", cspINumber);
        db.SetNullableString(command, "selfEmployedInd", "");
        db.SetNullableString(command, "sendTo", sendTo);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "note2", "");
      });

    entities.IncomeSource.Identifier = identifier;
    entities.IncomeSource.Type1 = type1;
    entities.IncomeSource.LastQtrIncome = lastQtrIncome;
    entities.IncomeSource.LastQtr = lastQtr;
    entities.IncomeSource.LastQtrYr = lastQtrYr;
    entities.IncomeSource.Attribute2NdQtrIncome = attribute2NdQtrIncome;
    entities.IncomeSource.Attribute2NdQtr = attribute2NdQtr;
    entities.IncomeSource.Attribute2NdQtrYr = attribute2NdQtrYr;
    entities.IncomeSource.Attribute3RdQtrIncome = attribute3RdQtrIncome;
    entities.IncomeSource.Attribute3RdQtr = attribute3RdQtr;
    entities.IncomeSource.Attribute3RdQtrYr = attribute3RdQtrYr;
    entities.IncomeSource.Attribute4ThQtrIncome = attribute4ThQtrIncome;
    entities.IncomeSource.Attribute4ThQtr = attribute4ThQtr;
    entities.IncomeSource.Attribute4ThQtrYr = attribute4ThQtrYr;
    entities.IncomeSource.SentDt = sentDt;
    entities.IncomeSource.ReturnDt = returnDt;
    entities.IncomeSource.ReturnCd = returnCd;
    entities.IncomeSource.Name = name;
    entities.IncomeSource.LastUpdatedTimestamp = identifier;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.CreatedTimestamp = identifier;
    entities.IncomeSource.CreatedBy = lastUpdatedBy;
    entities.IncomeSource.Code = code;
    entities.IncomeSource.CspINumber = cspINumber;
    entities.IncomeSource.SendTo = sendTo;
    entities.IncomeSource.WorkerId = workerId;
    entities.IncomeSource.StartDt = startDt;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Note = note;
    entities.IncomeSource.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 4);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 5);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var federalInd = import.CsePerson.FederalInd ?? "";

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "federalInd", federalInd);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.FederalInd = federalInd;
    entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea blank;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private IncomeSource incomeSource;
    private CsePerson csePerson;
  }
#endregion
}
