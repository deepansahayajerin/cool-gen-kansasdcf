// Program: SI_ADD_NCP_NON_COOP, ID: 1902537689, model: 746.
// Short name: SWE03754
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_ADD_NCP_NON_COOP.
/// </summary>
[Serializable]
public partial class SiAddNcpNonCoop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ADD_NCP_NON_COOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAddNcpNonCoop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAddNcpNonCoop.
  /// </summary>
  public SiAddNcpNonCoop(IContext context, Import import, Export export):
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
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // 04/06/2016  DDupree		Initial Development
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // ---------------------------------------------------------
    // Properties of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------------------
    // Properties of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCaseRoleCsePerson())
    {
      ExitState = "AP_NF_RB";

      return;
    }

    if (ReadCsePerson())
    {
      try
      {
        CreateNcpNonCooperation();

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

        local.DateWorkArea.Date = import.NcpNonCooperation.Letter1Date;
        local.Infrastructure.ReasonCode = "NCPLTR1ADD";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.EventId = 46;
        local.Infrastructure.BusinessObjectCd = "CAU";
        local.Infrastructure.CaseNumber = import.Case1.Number;
        local.Infrastructure.UserId = "NCOP";
        local.Infrastructure.ReferenceDate = local.Current.Date;
        UseCabConvertDate2String();
        local.TextWorkArea.Text30 = "Letter 1 date added for AP:";
        local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
          (entities.ApCsePerson.Number) + "; eff date: " + local
          .TextWorkArea.Text8;

        if (!IsEmpty(import.NcpNonCooperation.Letter1Code))
        {
          local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
            (entities.ApCsePerson.Number) + "; eff date: " + local
            .TextWorkArea.Text8 + "; code: " + (
              import.NcpNonCooperation.Letter1Code ?? "");
        }

        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        local.CaseUnitFound.Flag = "N";

        foreach(var item in ReadCaseUnit())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          local.CaseUnitFound.Flag = "Y";
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }
        }

        if (AsChar(local.CaseUnitFound.Flag) == 'N')
        {
          UseSpCabCreateInfrastructure();
        }

        if (Lt(local.Blank.Date, import.NcpNonCooperation.Phone1Date))
        {
          local.Infrastructure.ReasonCode = "NCPPHONE1ADD";
          local.DateWorkArea.Date = import.NcpNonCooperation.Phone1Date;
          UseCabConvertDate2String();
          local.TextWorkArea.Text30 = "Phone 1 date added for AP:";
          local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
          local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
            (entities.ApCsePerson.Number) + "; with a date of: " + local
            .TextWorkArea.Text8;

          if (!IsEmpty(import.NcpNonCooperation.Phone1Code))
          {
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with date: " + local
              .TextWorkArea.Text8 + "; with code: " + (
                import.NcpNonCooperation.Phone1Code ?? "");
          }

          local.Infrastructure.CreatedBy = global.UserId;
          local.Infrastructure.CreatedTimestamp = Now();
          local.CaseUnitFound.Flag = "N";

          foreach(var item in ReadCaseUnit())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.CaseUnitFound.Flag = "Y";
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.CaseUnitFound.Flag) == 'N')
          {
            UseSpCabCreateInfrastructure();
          }
        }

        if (Lt(local.Blank.Date, import.NcpNonCooperation.Letter2Date))
        {
          local.Infrastructure.ReasonCode = "NCPLTR2ADD";
          local.DateWorkArea.Date = import.NcpNonCooperation.Letter2Date;
          UseCabConvertDate2String();
          local.TextWorkArea.Text30 = "Letter 2 date added for AP:";
          local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
          local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
            (entities.ApCsePerson.Number) + "; with a date of: " + local
            .TextWorkArea.Text8;

          if (!IsEmpty(import.NcpNonCooperation.Letter2Code))
          {
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with date: " + local
              .TextWorkArea.Text8 + "; with code: " + (
                import.NcpNonCooperation.Letter2Code ?? "");
          }

          local.Infrastructure.CreatedBy = global.UserId;
          local.Infrastructure.CreatedTimestamp = Now();
          local.CaseUnitFound.Flag = "N";

          foreach(var item in ReadCaseUnit())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.CaseUnitFound.Flag = "Y";
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.CaseUnitFound.Flag) == 'N')
          {
            UseSpCabCreateInfrastructure();
          }
        }

        if (Lt(local.Blank.Date, import.NcpNonCooperation.Phone2Date))
        {
          local.Infrastructure.ReasonCode = "NCPPHONE2ADD";
          local.DateWorkArea.Date = import.NcpNonCooperation.Phone2Date;
          UseCabConvertDate2String();
          local.TextWorkArea.Text30 = "Phone 2 date added for AP:";
          local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
          local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
            (entities.ApCsePerson.Number) + "; with a date of: " + local
            .TextWorkArea.Text8;

          if (!IsEmpty(import.NcpNonCooperation.Phone2Code))
          {
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with date: " + local
              .TextWorkArea.Text8 + "; with code: " + (
                import.NcpNonCooperation.Phone2Code ?? "");
          }

          local.Infrastructure.CreatedBy = global.UserId;
          local.Infrastructure.CreatedTimestamp = Now();
          local.CaseUnitFound.Flag = "N";

          foreach(var item in ReadCaseUnit())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.CaseUnitFound.Flag = "Y";
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.CaseUnitFound.Flag) == 'N')
          {
            UseSpCabCreateInfrastructure();
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "NCP_NON_COOP_ALREADY_EXISTS";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "NCP_NON_COOP_PVV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "AP_FOR_CASE_NF";
    }
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void CreateNcpNonCooperation()
  {
    var ncpStatusCode = import.NcpNonCooperation.NcpStatusCode ?? "";
    var effectiveDate = local.Max.Date;
    var reasonCode = import.NcpNonCooperation.ReasonCode ?? "";
    var letter1Date = import.NcpNonCooperation.Letter1Date;
    var letter1Code = import.NcpNonCooperation.Letter1Code ?? "";
    var letter2Date = import.NcpNonCooperation.Letter2Date;
    var letter2Code = import.NcpNonCooperation.Letter2Code ?? "";
    var phone1Date = import.NcpNonCooperation.Phone1Date;
    var phone1Code = import.NcpNonCooperation.Phone1Code ?? "";
    var phone2Date = import.NcpNonCooperation.Phone2Date;
    var phone2Code = import.NcpNonCooperation.Phone2Code ?? "";
    var endStatusCode = import.NcpNonCooperation.EndStatusCode ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var casNumber = entities.Case1.Number;
    var cspNumber = entities.ApCsePerson.Number;
    var note = import.NcpNonCooperation.Note ?? "";

    entities.NcpNonCooperation.Populated = false;
    Update("CreateNcpNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "ncpStatusCd", ncpStatusCode);
        db.SetNullableDate(command, "effectiveDt", effectiveDate);
        db.SetNullableString(command, "reasonCd", reasonCode);
        db.SetNullableDate(command, "letter1Dt", letter1Date);
        db.SetNullableString(command, "letter1Cd", letter1Code);
        db.SetNullableDate(command, "letter2Dt", letter2Date);
        db.SetNullableString(command, "letter2Cd", letter2Code);
        db.SetNullableDate(command, "phone1Dt", phone1Date);
        db.SetNullableString(command, "phone1Cd", phone1Code);
        db.SetNullableDate(command, "phone2Dt", phone2Date);
        db.SetNullableString(command, "phone2Cd", phone2Code);
        db.SetNullableDate(command, "endDt", effectiveDate);
        db.SetNullableString(command, "endStatusCd", endStatusCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "note", note);
      });

    entities.NcpNonCooperation.NcpStatusCode = ncpStatusCode;
    entities.NcpNonCooperation.EffectiveDate = effectiveDate;
    entities.NcpNonCooperation.ReasonCode = reasonCode;
    entities.NcpNonCooperation.Letter1Date = letter1Date;
    entities.NcpNonCooperation.Letter1Code = letter1Code;
    entities.NcpNonCooperation.Letter2Date = letter2Date;
    entities.NcpNonCooperation.Letter2Code = letter2Code;
    entities.NcpNonCooperation.Phone1Date = phone1Date;
    entities.NcpNonCooperation.Phone1Code = phone1Code;
    entities.NcpNonCooperation.Phone2Date = phone2Date;
    entities.NcpNonCooperation.Phone2Code = phone2Code;
    entities.NcpNonCooperation.EndDate = effectiveDate;
    entities.NcpNonCooperation.EndStatusCode = endStatusCode;
    entities.NcpNonCooperation.CreatedBy = createdBy;
    entities.NcpNonCooperation.CreatedTimestamp = createdTimestamp;
    entities.NcpNonCooperation.LastUpdatedBy = createdBy;
    entities.NcpNonCooperation.LastUpdatedTimestamp = createdTimestamp;
    entities.NcpNonCooperation.CasNumber = casNumber;
    entities.NcpNonCooperation.CspNumber = cspNumber;
    entities.NcpNonCooperation.Note = note;
    entities.NcpNonCooperation.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCsePerson.Type1 = db.GetString(reader, 6);
        entities.ApCsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
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
    /// A value of NcpNonCooperation.
    /// </summary>
    [JsonPropertyName("ncpNonCooperation")]
    public NcpNonCooperation NcpNonCooperation
    {
      get => ncpNonCooperation ??= new();
      set => ncpNonCooperation = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private NcpNonCooperation ncpNonCooperation;
    private CsePersonsWorkSet ap;
    private Case1 case1;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
    }

    private DateWorkArea blank;
    private DateWorkArea max;
    private DateWorkArea dateWorkArea;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private TextWorkArea textWorkArea;
    private Common caseUnitFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NcpNonCooperation.
    /// </summary>
    [JsonPropertyName("ncpNonCooperation")]
    public NcpNonCooperation NcpNonCooperation
    {
      get => ncpNonCooperation ??= new();
      set => ncpNonCooperation = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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

    private NcpNonCooperation ncpNonCooperation;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
  }
#endregion
}
