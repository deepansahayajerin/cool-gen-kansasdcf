// Program: SI_INCS_UPDATE_INCOME_SOURCE, ID: 371763123, model: 746.
// Short name: SWE01250
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INCS_UPDATE_INCOME_SOURCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD updates the details of a person's income source.
/// </para>
/// </summary>
[Serializable]
public partial class SiIncsUpdateIncomeSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_UPDATE_INCOME_SOURCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsUpdateIncomeSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsUpdateIncomeSource.
  /// </summary>
  public SiIncsUpdateIncomeSource(IContext context, Import import, Export export)
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
    // 	                 	Income Source to Cse Person.
    // 10/20/98  W. Campbell           Modified code so that end date
    //                                 
    // will be returned to the USEing
    //                                 
    // action block correctly.
    // ------------------------------------------------------------
    // ---------------------------------------------
    // This PAD updates an income source for a CSE Person.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (IsEmpty(import.CsePerson.Number))
    {
      import.CsePerson.Number = import.CsePersonsWorkSet.Number;
    }

    if (ReadCsePerson())
    {
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

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CSE_PERSON_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadIncomeSource())
    {
      // ---------------------------------------------
      // 10/20/98 W. Campbell  Moved the following
      // IF stmt and SET stmt inside it to here to only
      // set the end dt when ready for it.
      // ---------------------------------------------
      if (!Lt(local.Blank.Date, import.IncomeSource.EndDt))
      {
        import.IncomeSource.EndDt = UseCabSetMaximumDiscontinueDate1();
      }

      try
      {
        UpdateIncomeSource();
        import.IncomeSource.Assign(entities.IncomeSource);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ---------------------------------------------
      // 10/20/98 W. Campbell - Moved the following
      // two SET stmts to here to reset the end dt
      // after all possible returns from UPDATE.
      // ---------------------------------------------
      local.DateWorkArea.Date = import.IncomeSource.EndDt;
      import.IncomeSource.EndDt = UseCabSetMaximumDiscontinueDate2();
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
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

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 7);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.IncomeSource.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 10);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.IncomeSource.Attribute4ThQtr =
          db.GetNullableString(reader, 12);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 13);
        entities.IncomeSource.SentDt = db.GetNullableDate(reader, 14);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 16);
        entities.IncomeSource.Name = db.GetNullableString(reader, 17);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.IncomeSource.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.IncomeSource.CreatedBy = db.GetString(reader, 21);
        entities.IncomeSource.Code = db.GetNullableString(reader, 22);
        entities.IncomeSource.CspINumber = db.GetString(reader, 23);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 24);
        entities.IncomeSource.WorkerId = db.GetNullableString(reader, 25);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 26);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 27);
        entities.IncomeSource.Note = db.GetNullableString(reader, 28);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
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

  private void UpdateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

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
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var code = import.IncomeSource.Code ?? "";
    var sendTo = import.IncomeSource.SendTo ?? "";
    var workerId = import.IncomeSource.WorkerId ?? "";
    var startDt = import.IncomeSource.StartDt;
    var endDt = import.IncomeSource.EndDt;
    var note = import.IncomeSource.Note ?? "";

    CheckValid<IncomeSource>("SendTo", sendTo);
    entities.IncomeSource.Populated = false;
    Update("UpdateIncomeSource",
      (db, command) =>
      {
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
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "code", code);
        db.SetNullableString(command, "sendTo", sendTo);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

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
    entities.IncomeSource.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.Code = code;
    entities.IncomeSource.SendTo = sendTo;
    entities.IncomeSource.WorkerId = workerId;
    entities.IncomeSource.StartDt = startDt;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Note = note;
    entities.IncomeSource.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkArea blank;
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
