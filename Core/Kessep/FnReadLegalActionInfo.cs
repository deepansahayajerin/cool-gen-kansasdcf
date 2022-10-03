// Program: FN_READ_LEGAL_ACTION_INFO, ID: 372095445, model: 746.
// Short name: SWE01591
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_READ_LEGAL_ACTION_INFO.
/// </summary>
[Serializable]
public partial class FnReadLegalActionInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_LEGAL_ACTION_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadLegalActionInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadLegalActionInfo.
  /// </summary>
  public FnReadLegalActionInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************************
    // Date	  Programmer		Ref	Description
    // ------------------------------------------------------------------------
    // 08/06/96  Holly Kennedy-MTW		Initial code.  This action block
    // 					was added to allow the screen
    // 					to be automatically populated to
    // 					match the legal action data when
    // 					flowing from the Legal Detail
    // 					screen.
    // 03/20/97 R.B.Mohapatra  Included the views and set statements to populate
    // the correct attributes in Export Obligation_payment_schedule
    // ************************************************************************
    // ***--- Sumanta - MTW - 05/01/97
    // ***--- Changed the use of DB2 current date to IEF current date..
    // ***---
    // ****************************************************************
    // *** Bud Adams       8/26/98  Added 'organization_type' to both
    //                              CSE_Person and Supported_Person
    //                              CSE_Person so view match would be OK.
    // ****************************************************************
    // =================================================
    // 10/23/98 - B Adams  -  Deleted use fn-hardcoded-debt-distribution
    //   and use fn-hardcoded-legal and imported the 5 views.
    // 1/8/99 - b adams  -  Read properties set
    // 11/3/99 - E. Parker 	Put fn_read_case_no_and_worker_id back in, because 
    // we don't care about finding a specific case, we just want to find any
    // case.
    // =================================================
    // =================================================================================
    // 06/22/2006               GVandy              WR# 230751
    // Add capability to select tribal interstate request.
    // ===================================================================================
    local.HardcodedJudgement.Type1 = "J";

    if (ReadLegalAction())
    {
      export.LegalAction.Assign(entities.LegalAction);
      export.DebtDetail.DueDt = entities.LegalActionDetail.EffectiveDate;

      if (AsChar(entities.LegalAction.Type1) == AsChar
        (local.HardcodedJudgement.Type1))
      {
        ExitState = "FN0000_LEG_ACT_MUST_BE_JUDGEMENT";

        return;
      }

      if (ReadLegalActionDetailObligationType())
      {
        export.ObligationAmount.TotalCurrency =
          entities.LegalActionDetail.JudgementAmount.GetValueOrDefault();
        export.DebtDetail.DueDt = entities.LegalActionDetail.EffectiveDate;
        export.ObligationType.Assign(entities.ObligationType);

        if (!IsEmpty(entities.LegalActionDetail.FreqPeriodCode))
        {
          export.ObligationPaymentSchedule.FrequencyCode =
            entities.LegalActionDetail.FreqPeriodCode ?? Spaces(2);
          export.ObligationPaymentSchedule.Amount =
            entities.LegalActionDetail.ArrearsAmount;
          export.ObligationPaymentSchedule.DayOfMonth1 =
            entities.LegalActionDetail.DayOfMonth1;
          export.ObligationPaymentSchedule.DayOfMonth2 =
            entities.LegalActionDetail.DayOfMonth2;
          export.ObligationPaymentSchedule.DayOfWeek =
            entities.LegalActionDetail.DayOfWeek;
          export.ObligationPaymentSchedule.StartDt =
            entities.LegalActionDetail.EffectiveDate;
          export.ObligationPaymentSchedule.EndDt =
            entities.LegalActionDetail.EndDate;
          export.ObligationPaymentSchedule.PeriodInd =
            entities.LegalActionDetail.PeriodInd;
        }
        else
        {
          export.ObligationPaymentSchedule.FrequencyCode = "";
        }

        // *****
        // Get Obligor and Concurrent Obligor
        // *****
        local.Common.Count = 0;

        foreach(var item in ReadLegalActionPersonCsePerson1())
        {
          ++local.Common.Count;

          // *****
          // If there are more than 2 obligors associated to the Legal Action 
          // Detail cause an error.  There should only be a primary and
          // secondary obligor in any situation.
          // *****
          switch(local.Common.Count)
          {
            case 1:
              export.ObligorCsePerson.Number = entities.CsePerson.Number;

              foreach(var item1 in ReadLaPersonLaCaseRoleLegalActionCaseRole())
              {
                foreach(var item2 in ReadCaseCaseRole())
                {
                  export.InterstateRequest.Assign(entities.InterstateRequest);

                  if (!IsEmpty(export.InterstateRequest.Country))
                  {
                    local.CodeValue.Cdvalue =
                      export.InterstateRequest.Country ?? Spaces(10);
                    local.Code.CodeName = "COUNTRY CODE";
                    UseCabValidateCodeValue();

                    if (AsChar(local.Error.Flag) == 'N')
                    {
                      export.Country.Description = "COUNTRY CODE NOT FOUND";
                    }
                  }

                  if (!IsEmpty(export.InterstateRequest.TribalAgency))
                  {
                    local.CodeValue.Cdvalue =
                      export.InterstateRequest.TribalAgency ?? Spaces(10);
                    local.Code.CodeName = "TRIBAL IV-D AGENCIES";
                    UseCabValidateCodeValue();

                    if (AsChar(local.Error.Flag) == 'N')
                    {
                      export.Country.Description =
                        "TRIBAL AGENCY CODE NOT FOUND";
                    }
                  }
                }
              }

              break;
            case 2:
              export.ConcurrentCsePerson.Number = entities.CsePerson.Number;

              break;
            default:
              ExitState = "FN0000_ONLY_2_OBLIGORS";

              return;
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "FN0008_OBLIGOR_NOT_PRESENT";

          return;
        }

        // *****
        // Get Supported Person information
        // *****
        local.Common.Count = 0;

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadLegalActionPersonCsePerson2())
        {
          export.Export1.Update.SupportedCsePerson.Number =
            entities.SupportedPerson.Number;
          export.Export1.Update.ObligationTransaction.Amount =
            entities.LegalActionPerson.JudgementAmount.GetValueOrDefault();
          export.Export1.Update.DebtDetail.CoveredPrdStartDt =
            entities.LegalActionPerson.EffectiveDate;

          if (Equal(entities.LegalActionPerson.EndDate, import.Max.Date))
          {
            export.Export1.Update.DebtDetail.CoveredPrdEndDt =
              entities.LegalActionDetail.EndDate;
          }
          else
          {
            export.Export1.Update.DebtDetail.CoveredPrdEndDt =
              entities.LegalActionPerson.EndDate;
          }

          // =================================================
          // 7/15/99 - Bud Adams  -  If the person is ACTIVE on a case,
          //   get that case number.  If they aren't, then check to see if
          //   they are END DATED on a case and get that one.
          //   Changed the action block from FN_Read_Case_No_And_
          //   Worker_ID to this one; the proper case was not being
          //   returned.
          // =================================================
          local.Discontinue.Date = new DateTime(1960, 1, 1);
          UseFnReadCaseNoAndWorkerId();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("NO_CASE_RL_FOUND_FOR_SUPP_PERSON"))
            {
              // =================================================
              // 5/26/99 - bud adams  -  If this is a non-case related person
              //   set this flag so that when it comes time to create the
              //   obligation, no ob_tran is created for this person.
              // =================================================
              ExitState = "ACO_NN0000_ALL_OK";
              export.Export1.Update.SupportedCsePersonsWorkSet.Flag = "Z";
              export.Export1.Next();

              continue;
            }
            else if (IsExitState("OFFICE_SERVICE_PROVIDER_NF"))
            {
              // *****
              // Temporary fix until the assignment piece of the data model is 
              // resolved and put in place.
              // *****
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              export.Export1.Next();

              return;
            }
          }

          ++local.Common.Count;

          // :
          // ================================================
          // 2/11/1999 - b adams  -  replace the Read actions, etc., with
          // this CAB
          // ================================================
          // :
          // ================================================
          // 3/31/1999 - b adams  - Read _Program_For_Supp_Person was
          // removed.  Program code for supported person adds no
          // value or meaning - only confusion.
          // ================================================
          export.Export1.Next();
        }

        // =================================================
        // 7/16/99 - bud adams  -  If no case-related persons are found
        //   then no obligation transaction will be created - and we have
        //   no obligation to create.
        // =================================================
        if (local.Common.Count == 0)
        {
          ExitState = "FN0000_NO_SUPPORTD_PRSN_ON_LDET";

          return;
        }

        if (ReadCsePerson())
        {
          export.Alternate.Char2 = "LE";
          local.TextWorkArea.Text10 = entities.AlternateAddress.Number;
          UseEabPadLeftWithZeros();
          export.Alternate.Number = local.TextWorkArea.Text10;
        }
      }
      else
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Country.Description = useExport.CodeValue.Description;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnReadCaseNoAndWorkerId()
  {
    var useImport = new FnReadCaseNoAndWorkerId.Import();
    var useExport = new FnReadCaseNoAndWorkerId.Export();

    useImport.SearchDiscontinue.Date = local.Discontinue.Date;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.Supported.Number = export.Export1.Item.SupportedCsePerson.Number;

    Call(FnReadCaseNoAndWorkerId.Execute, useImport, useExport);

    export.Export1.Update.Case1.Number = useExport.Case1.Number;
    MoveServiceProvider(useExport.ServiceProvider,
      export.Export1.Update.ServiceProvider);
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId", entities.LegalActionCaseRole.CroIdentifier);
        db.SetString(command, "type", entities.LegalActionCaseRole.CroType);
        db.SetString(
          command, "cspNumber1", entities.LegalActionCaseRole.CspNumber);
        db.SetString(
          command, "casNumber", entities.LegalActionCaseRole.CasNumber);
        db.SetString(command, "cspNumber2", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.AlternateAddress.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AlternateAddress.Number = db.GetString(reader, 0);
        entities.AlternateAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLaPersonLaCaseRoleLegalActionCaseRole()
  {
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadLaPersonLaCaseRoleLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 9);
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Type1 = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 5);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetailObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 11);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 12);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "accountType", import.HcObligor.AccountType ?? "");
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CsePerson.Type1 = db.GetString(reader, 8);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 9);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "accountType", import.HcSupported.AccountType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.SupportedPerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 7);
        entities.SupportedPerson.Type1 = db.GetString(reader, 8);
        entities.SupportedPerson.OrganizationName =
          db.GetNullableString(reader, 9);
        entities.LegalActionPerson.Populated = true;
        entities.SupportedPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.SupportedPerson.Type1);

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of HcSupported.
    /// </summary>
    [JsonPropertyName("hcSupported")]
    public LegalActionPerson HcSupported
    {
      get => hcSupported ??= new();
      set => hcSupported = value;
    }

    /// <summary>
    /// A value of HcObligor.
    /// </summary>
    [JsonPropertyName("hcObligor")]
    public LegalActionPerson HcObligor
    {
      get => hcObligor ??= new();
      set => hcObligor = value;
    }

    /// <summary>
    /// A value of HcPgmNonAdcFosterCar.
    /// </summary>
    [JsonPropertyName("hcPgmNonAdcFosterCar")]
    public ProgramScreenAttributes HcPgmNonAdcFosterCar
    {
      get => hcPgmNonAdcFosterCar ??= new();
      set => hcPgmNonAdcFosterCar = value;
    }

    /// <summary>
    /// A value of HcPgmAdc.
    /// </summary>
    [JsonPropertyName("hcPgmAdc")]
    public ProgramScreenAttributes HcPgmAdc
    {
      get => hcPgmAdc ??= new();
      set => hcPgmAdc = value;
    }

    /// <summary>
    /// A value of HcPgmAdcFosterCare.
    /// </summary>
    [JsonPropertyName("hcPgmAdcFosterCare")]
    public ProgramScreenAttributes HcPgmAdcFosterCare
    {
      get => hcPgmAdcFosterCare ??= new();
      set => hcPgmAdcFosterCare = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private LegalActionPerson hcSupported;
    private LegalActionPerson hcObligor;
    private ProgramScreenAttributes hcPgmNonAdcFosterCar;
    private ProgramScreenAttributes hcPgmAdc;
    private ProgramScreenAttributes hcPgmAdcFosterCare;
    private DateWorkArea max;
    private DateWorkArea current;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public Program Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of SupportedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedCsePerson")]
      public CsePerson SupportedCsePerson
      {
        get => supportedCsePerson ??= new();
        set => supportedCsePerson = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
      }

      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of Concurrent.
      /// </summary>
      [JsonPropertyName("concurrent")]
      public ObligationTransaction Concurrent
      {
        get => concurrent ??= new();
        set => concurrent = value;
      }

      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public ObligationTransaction Prev
      {
        get => prev ??= new();
        set => prev = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public DebtDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdel;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private DebtDetail debtDetail;
      private ServiceProvider serviceProvider;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ObligationTransaction concurrent;
      private ObligationTransaction prev;
      private Common common;
      private DebtDetail hidden;
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentCsePersonsWorkSet
    {
      get => concurrentCsePersonsWorkSet ??= new();
      set => concurrentCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of ObligationAmount.
    /// </summary>
    [JsonPropertyName("obligationAmount")]
    public Common ObligationAmount
    {
      get => obligationAmount ??= new();
      set => obligationAmount = value;
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
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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
    private CodeValue country;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePersonsWorkSet concurrentCsePersonsWorkSet;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Obligation obligation;
    private CsePersonsWorkSet supported;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private Common obligationAmount;
    private LegalAction legalAction;
    private CsePerson concurrentCsePerson;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet alternate;
    private Array<ExportGroup> export1;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of ForDebtScreens.
    /// </summary>
    [JsonPropertyName("forDebtScreens")]
    public Common ForDebtScreens
    {
      get => forDebtScreens ??= new();
      set => forDebtScreens = value;
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
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    /// <summary>
    /// A value of HardcodedJudgement.
    /// </summary>
    [JsonPropertyName("hardcodedJudgement")]
    public LegalAction HardcodedJudgement
    {
      get => hardcodedJudgement ??= new();
      set => hardcodedJudgement = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public CsePerson Alt
    {
      get => alt ??= new();
      set => alt = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of HardcodeSpouse.
    /// </summary>
    [JsonPropertyName("hardcodeSpouse")]
    public CaseRole HardcodeSpouse
    {
      get => hardcodeSpouse ??= new();
      set => hardcodeSpouse = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of HardcodeChild.
    /// </summary>
    [JsonPropertyName("hardcodeChild")]
    public CaseRole HardcodeChild
    {
      get => hardcodeChild ??= new();
      set => hardcodeChild = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      forDebtScreens = null;
      textWorkArea = null;
      hardcodedJudgement = null;
      common = null;
      alt = null;
      csePersonsWorkSet = null;
      current = null;
      hardcodeSpouse = null;
      supported = null;
      hardcodeChild = null;
      codeValue = null;
      code = null;
      error = null;
    }

    private Common forDebtScreens;
    private TextWorkArea textWorkArea;
    private DateWorkArea discontinue;
    private LegalAction hardcodedJudgement;
    private Common common;
    private CsePerson alt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private CsePerson obligor;
    private CaseRole hardcodeSpouse;
    private CsePerson supported;
    private CaseRole hardcodeChild;
    private CodeValue codeValue;
    private Code code;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of AlternateAddress.
    /// </summary>
    [JsonPropertyName("alternateAddress")]
    public CsePerson AlternateAddress
    {
      get => alternateAddress ??= new();
      set => alternateAddress = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePerson SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
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
    /// A value of ApType.
    /// </summary>
    [JsonPropertyName("apType")]
    public CaseRole ApType
    {
      get => apType ??= new();
      set => apType = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private CsePerson alternateAddress;
    private Tribunal tribunal;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private ObligationTransaction obligationTransaction;
    private CsePerson supportedPerson;
    private Case1 case1;
    private CaseRole apType;
    private CsePerson obligor;
    private CaseRole caseRole;
    private CsePerson supported;
    private CaseRole child;
    private InterstateRequest interstateRequest;
  }
#endregion
}
