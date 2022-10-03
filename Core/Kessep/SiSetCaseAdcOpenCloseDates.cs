// Program: SI_SET_CASE_ADC_OPEN_CLOSE_DATES, ID: 371787556, model: 746.
// Short name: SWE01922
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
/// A program: SI_SET_CASE_ADC_OPEN_CLOSE_DATES.
/// </para>
/// <para>
/// This CAB will maintain the ADC Open and Closed Dates for Cases.
/// </para>
/// </summary>
[Serializable]
public partial class SiSetCaseAdcOpenCloseDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_SET_CASE_ADC_OPEN_CLOSE_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiSetCaseAdcOpenCloseDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiSetCaseAdcOpenCloseDates.
  /// </summary>
  public SiSetCaseAdcOpenCloseDates(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date    Developer       Request #       Description
    // 05/12/97  Sid Chowdhary	  IDCR # 230    Initial Development
    // 09/15/97  Siraj Konkader
    // Corrected event id values.
    // Added IF EX ST <> ALL OK after SP_CAB_CREATE_INFR...
    // 01/08/98  Jack Rookard
    // Removed code for reading each Case Unit during
    // Infrastructure generation.  It has been determined
    // that this action block produces
    // Case level events.  Also removed references
    // to SP Cab Get Next Situation Number.  Situation
    // Number is now set to 0 prior to call to
    // SP Cab Create Infrastructure.
    // ----------------------------------------------------------
    // ****************************************************************
    // 11/23/1998    C. Ott    Move Import Interface Person Program Data to 
    // Local Person Program view.
    // ****************************************************************
    local.Program.Code = import.InterfacePersonProgram.ProgramCode;
    export.CntlTotAdcCaseCloses.Count = import.CntlTotAdcCaseCloses.Count;
    export.CntlTotAdcCaseOpens.Count = import.CntlTotAdcCaseOpens.Count;
    UseCabSetMaximumDiscontinueDate();
    local.ProgramProcessing.Date = import.ProgramProcessingInfo.ProcessDate;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (AsChar(import.OpenClosedAdc.Flag) == 'C')
    {
      foreach(var item in ReadCase())
      {
        if (Equal(entities.Case1.AdcCloseDate, local.Max.Date))
        {
          UseSiReadCaseProgramType();

          if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
          {
            // -----------------------------------------------
            // This exit state is used by the online programs
            // and is causing the batch to rollback.
            // -----------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
          }

          // ¼¼¼¼¼¼¼¼¼¼¼¼
          // Added IF stmt... SAK 7/28/97
          // ©©©©©©©©©©©©
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (!Equal(local.Program.Code, "AF") && !IsEmpty(local.Program.Code))
          {
            try
            {
              UpdateCase1();
              ++export.CntlTotAdcCaseCloses.Count;
              ++import.ExpCheckpointNumbUpdates.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CASE_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CASE_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // *** Insert Event for Case Program Change ***
            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.EventId = 5;
            local.Infrastructure.UserId = "PEPR";
            local.Infrastructure.ReasonCode = "CASETONADC";
            local.Infrastructure.BusinessObjectCd = "CAS";
            local.Infrastructure.ReferenceDate =
              import.InterfacePersonProgram.ProgramEndDate;
            local.ConvertDateDateWorkArea.Date =
              import.InterfacePersonProgram.ProgramEndDate;
            UseCabConvertDate2String();
            local.Infrastructure.Detail =
              "The ADC Case has been closed. ADC Program end date :" + TrimEnd
              (local.ConvertDateTextWorkArea.Text8);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + ";" + " Reason :" + TrimEnd
              (import.InterfacePersonProgram.ClosureReason);

            // --------------------------------------------
            // Assigning global infrastructure attribute values
            // --------------------------------------------
            local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            local.Infrastructure.CaseUnitNumber = 0;

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

            UseSpCabCreateInfrastructure();

            // ¼¼¼¼¼¼¼¼¼¼¼¼
            // Added IF stmt... SAK 9/15/97
            // ©©©©©©©©©©©©
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // *** END Insert Event for Case Program Change ***
          }
        }
        else
        {
          // Continue. ADC Case already closed.
        }
      }
    }

    if (AsChar(import.OpenClosedAdc.Flag) == 'O')
    {
      foreach(var item in ReadCase())
      {
        if (!Lt(import.ProgramProcessingInfo.ProcessDate,
          entities.Case1.AdcOpenDate) && !
          Lt(entities.Case1.AdcCloseDate,
          import.ProgramProcessingInfo.ProcessDate))
        {
          // Continue. ADC Case already Open.
        }
        else
        {
          UseSiReadCaseProgramType();

          if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
          {
            // -----------------------------------------------
            // This exit state is used by the online programs
            // and is causing the batch to rollback.
            // -----------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
          }

          // ¼¼¼¼¼¼¼¼¼¼¼¼
          // Added IF stmt... SAK 7/28/97
          // ©©©©©©©©©©©©
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(local.Program.Code, "AF"))
          {
            try
            {
              UpdateCase2();
              ++import.ExpCheckpointNumbUpdates.Count;
              ++export.CntlTotAdcCaseOpens.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CASE_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CASE_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // *** Insert Event for Case Program Change ***
            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.EventId = 5;
            local.Infrastructure.UserId = "PEPR";
            local.Infrastructure.ReasonCode = "CASETOADC";
            local.Infrastructure.BusinessObjectCd = "CAS";
            local.Infrastructure.ReferenceDate =
              import.InterfacePersonProgram.ProgEffectiveDate;
            local.ConvertDateDateWorkArea.Date =
              import.InterfacePersonProgram.ProgEffectiveDate;
            UseCabConvertDate2String();
            local.Infrastructure.Detail =
              "The ADC Case has been opened. ADC Program effective date :" + TrimEnd
              (local.ConvertDateTextWorkArea.Text8);

            // --------------------------------------------
            // Assigning global infrastructure attribute
            //   values
            // --------------------------------------------
            local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            local.Infrastructure.CaseUnitNumber = 0;

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

            // Modified call to Sp_Cab_Create_Infra to return its export view 
            // back to this action block.
            // This facilitates our strategy to remove the use of the Control 
            // Table from Infra creation.
            // J.Rookard 12/18/97
            UseSpCabCreateInfrastructure();

            // ¼¼¼¼¼¼¼¼¼¼¼¼
            // Added IF stmt... SAK 9/15/97
            // ©©©©©©©©©©©©
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // *** END Insert Event for Case Program Change ***
          }
        }
      }
    }

    if (AsChar(import.OpenClosedAdc.Flag) == 'D')
    {
      foreach(var item in ReadCase())
      {
        if (Equal(entities.Case1.AdcCloseDate, local.Max.Date))
        {
          try
          {
            UpdateCase1();
            ++import.ExpCheckpointNumbUpdates.Count;
            ++export.CntlTotAdcCaseCloses.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // *** Insert Event for ADC Case Closed ***
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.EventId = 5;
          local.Infrastructure.UserId = "PEPR";
          local.Infrastructure.ReasonCode = "ADC_CLOSED";
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.ReferenceDate =
            import.InterfacePersonProgram.ProgramEndDate;
          local.ConvertDateDateWorkArea.Date =
            import.InterfacePersonProgram.ProgramEndDate;
          UseCabConvertDate2String();
          local.Infrastructure.Detail =
            "The ADC Case has been closed. ADC Program end date :" + TrimEnd
            (local.ConvertDateTextWorkArea.Text8);
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + ";"
            + " Reason :" + TrimEnd
            (import.InterfacePersonProgram.ClosureReason);

          // --------------------------------------------
          // Assigning global infrastructure attribute values
          // --------------------------------------------
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = 0;

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

          UseSpCabCreateInfrastructure();

          // ¼¼¼¼¼¼¼¼¼¼¼¼
          // Added IF stmt... SAK 9/15/97
          // ©©©©©©©©©©©©
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // *** END Insert Event for Case Program Change ***
        }
        else
        {
          // Continue. ADC Case already closed.
        }
      }
    }
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ConvertDateDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ConvertDateTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.ProgramProcessing.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 1);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Case1.AdcOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.AdcCloseDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
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

  private void UpdateCase1()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var adcCloseDate = import.InterfacePersonProgram.ProgramEndDate;

    entities.Case1.Populated = false;
    Update("UpdateCase1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "adcCloseDate", adcCloseDate);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.AdcCloseDate = adcCloseDate;
    entities.Case1.Populated = true;
  }

  private void UpdateCase2()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var adcOpenDate = import.InterfacePersonProgram.ProgEffectiveDate;
    var adcCloseDate = local.Max.Date;

    entities.Case1.Populated = false;
    Update("UpdateCase2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "adcOpenDate", adcOpenDate);
        db.SetNullableDate(command, "adcCloseDate", adcCloseDate);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.AdcOpenDate = adcOpenDate;
    entities.Case1.AdcCloseDate = adcCloseDate;
    entities.Case1.Populated = true;
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
    /// A value of ExpCheckpointNumbUpdates.
    /// </summary>
    [JsonPropertyName("expCheckpointNumbUpdates")]
    public Common ExpCheckpointNumbUpdates
    {
      get => expCheckpointNumbUpdates ??= new();
      set => expCheckpointNumbUpdates = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseOpens.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseOpens")]
    public Common CntlTotAdcCaseOpens
    {
      get => cntlTotAdcCaseOpens ??= new();
      set => cntlTotAdcCaseOpens = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseCloses.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseCloses")]
    public Common CntlTotAdcCaseCloses
    {
      get => cntlTotAdcCaseCloses ??= new();
      set => cntlTotAdcCaseCloses = value;
    }

    /// <summary>
    /// A value of OpenClosedAdc.
    /// </summary>
    [JsonPropertyName("openClosedAdc")]
    public Common OpenClosedAdc
    {
      get => openClosedAdc ??= new();
      set => openClosedAdc = value;
    }

    /// <summary>
    /// A value of InterfacePersonProgram.
    /// </summary>
    [JsonPropertyName("interfacePersonProgram")]
    public InterfacePersonProgram InterfacePersonProgram
    {
      get => interfacePersonProgram ??= new();
      set => interfacePersonProgram = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common expCheckpointNumbUpdates;
    private Common cntlTotAdcCaseOpens;
    private Common cntlTotAdcCaseCloses;
    private Common openClosedAdc;
    private InterfacePersonProgram interfacePersonProgram;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CntlTotAdcCaseOpens.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseOpens")]
    public Common CntlTotAdcCaseOpens
    {
      get => cntlTotAdcCaseOpens ??= new();
      set => cntlTotAdcCaseOpens = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseCloses.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseCloses")]
    public Common CntlTotAdcCaseCloses
    {
      get => cntlTotAdcCaseCloses ??= new();
      set => cntlTotAdcCaseCloses = value;
    }

    private Common cntlTotAdcCaseOpens;
    private Common cntlTotAdcCaseCloses;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProgramProcessing.
    /// </summary>
    [JsonPropertyName("programProcessing")]
    public DateWorkArea ProgramProcessing
    {
      get => programProcessing ??= new();
      set => programProcessing = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of ConvertDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateDateWorkArea")]
    public DateWorkArea ConvertDateDateWorkArea
    {
      get => convertDateDateWorkArea ??= new();
      set => convertDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateTextWorkArea")]
    public TextWorkArea ConvertDateTextWorkArea
    {
      get => convertDateTextWorkArea ??= new();
      set => convertDateTextWorkArea = value;
    }

    private DateWorkArea programProcessing;
    private Program program;
    private DateWorkArea max;
    private DateWorkArea blank;
    private Infrastructure infrastructure;
    private DateWorkArea convertDateDateWorkArea;
    private TextWorkArea convertDateTextWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseUnit caseUnit;
    private InterstateRequest interstateRequest;
  }
#endregion
}
