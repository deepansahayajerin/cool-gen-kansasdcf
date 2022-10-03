// Program: LE_CRED_DISPLAY_CRA_ACTIONS, ID: 372586590, model: 746.
// Short name: SWE00747
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
/// A program: LE_CRED_DISPLAY_CRA_ACTIONS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads and populates Credit Reporting action details for 
/// the obligor.
/// </para>
/// </summary>
[Serializable]
public partial class LeCredDisplayCraActions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CRED_DISPLAY_CRA_ACTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCredDisplayCraActions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCredDisplayCraActions.
  /// </summary>
  public LeCredDisplayCraActions(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 02/19/1996	govind				Initial Coding
    // 04/28/97        Ty Hill-MTW                     Change Current_date
    // 10/07/98        P. Sharp     Rounded amount fields to the whole dollar. 
    // Problme Report 25734
    // 06/07/97        D. Jean     Remove rounding, wrong solution for 25734
    // 08/16/01	E.Shirk		PR#122269	Added logic to retrieve 5 years of credit 
    // reporting action data.
    // 09/20/06        S.Newman	PR#245232	Removed logic to retrieve 5 years of 
    // credit reporting action data.
    // 12/05/17	GVandy		CQ56369		Add Good Cause and Exemption Indicators.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.EndDate.Date = import.End.CraTransDate;
    MoveCsePersonsWorkSet(import.Obligor, local.Obligor);
    MoveCsePersonsWorkSet(import.Obligor, export.Obligor);

    // ************************************************************************************
    // The screen will display 5 years of transactions.  Set beg and end date 
    // ranges.
    // ************************************************************************************
    if (Equal(import.End.CraTransDate, local.NullDate.Date))
    {
      local.EndDate.Date = new DateTime(2099, 12, 31);
      local.BegDate.Date = Now().Date.AddYears(-5);
    }
    else
    {
      local.BegDate.Date = AddYears(import.End.CraTransDate, -5);
    }

    if (!IsEmpty(import.Obligor.Number))
    {
      // ---------------------------------------------
      // Use cse person number even if ssn is supplied
      // ---------------------------------------------
    }
    else if (!IsEmpty(import.Obligor.Ssn))
    {
      local.SearchOption.Flag = "1";
      UseCabMatchCsePerson();

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        // ---------------------------------------------
        // Use the first entry returned
        // ---------------------------------------------
        MoveCsePersonsWorkSet(local.Group.Item.Detail, export.Obligor);

        break;
      }
    }
    else
    {
      // ---------------------------------------------
      // Both person # and SSN are spaces. Error.
      // ---------------------------------------------
      MoveCsePersonsWorkSet(local.Obligor, export.Obligor);
      export.Obligor.FormattedName = "";
      ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

      return;
    }

    if (IsEmpty(export.Obligor.Number))
    {
      // ---------------------------------------------
      // Person # was not supplied; ssn was supplied;
      // Could not identify the person for the ssn.
      // ---------------------------------------------
      MoveCsePersonsWorkSet(local.Obligor, export.Obligor);
      export.Obligor.FormattedName = "";
      ExitState = "LE0000_CSE_PERSON_NF_FOR_SSN";

      return;
    }

    if (ReadCsePerson())
    {
      UseSiReadCsePerson();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.NoOfAdmActCertRecs.Count = 0;

    if (ReadCreditReporting())
    {
      export.CreditReporting.Assign(entities.ExistingCred);
      ++local.NoOfAdmActCertRecs.Count;
      local.CrRptActnBegDate.Date = new DateTime(1, 1, 1);

      export.Creds.Index = 0;
      export.Creds.Clear();

      foreach(var item in ReadCreditReportingAction())
      {
        export.Creds.Update.CredsDetail.Assign(
          entities.ExistingCreditReportingAction);
        local.CurrentAmt.Count = 0;
        local.OrigAmt.Count = 0;
        local.HighAmt.Count = 0;
        export.Creds.Next();
      }
    }

    local.NoOfCseCases.Count = 0;

    // ---------------------------------------------
    // Read and move to a group view of more than 5 occurences to test for more 
    // than 5 cse cases. Then move the first five cse cases to export group
    // view.
    // ---------------------------------------------
    local.CseCases.Index = 0;
    local.CseCases.Clear();

    foreach(var item in ReadCase())
    {
      ++local.NoOfCseCases.Count;
      MoveCase1(entities.ExistingApCase, local.CseCases.Update.CseCasesDetail);
      local.CseCases.Next();
    }

    local.CseCaseGroupIndex.Count = 0;

    for(local.CseCases.Index = 0; local.CseCases.Index < local.CseCases.Count; ++
      local.CseCases.Index)
    {
      ++local.CseCaseGroupIndex.Count;

      switch(local.CseCaseGroupIndex.Count)
      {
        case 1:
          MoveCase1(local.CseCases.Item.CseCasesDetail, export.Export1St);

          break;
        case 2:
          MoveCase1(local.CseCases.Item.CseCasesDetail, export.Export2Nd);

          break;
        case 3:
          MoveCase1(local.CseCases.Item.CseCasesDetail, export.Export3Rd);

          break;
        case 4:
          MoveCase1(local.CseCases.Item.CseCasesDetail, export.Export4Th);

          break;
        case 5:
          MoveCase1(local.CseCases.Item.CseCasesDetail, export.Export5Th);

          break;
        default:
          break;
      }
    }

    if (local.NoOfCseCases.Count > 5)
    {
      export.MoreCseCasesInd.OneChar = "+";
    }

    if (local.NoOfAdmActCertRecs.Count == 0)
    {
      ExitState = "LE0000_NO_CRED_ADMIN_ACTION_TAKN";
    }

    // --Determine if any of the NCPs obligation are exempted from credit 
    // reporting.
    export.AdminExemptExists.Flag = "N";

    if (ReadObligationAdmActionExemption())
    {
      export.AdminExemptExists.Flag = "Y";
    }

    // --Determine if Good Cause has been claimed on any of the NCPs cases.
    export.GoodCauseExists.Flag = "N";

    foreach(var item in ReadCaseRole())
    {
      foreach(var item1 in ReadGoodCause())
      {
        if (Equal(entities.GoodCause.Code, "GC") || Equal
          (entities.GoodCause.Code, "PD"))
        {
          export.GoodCauseExists.Flag = "Y";

          return;
        }
        else
        {
          goto ReadEach;
        }
      }

ReadEach:
      ;
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveExport1ToGroup(CabMatchCsePerson.Export.
    ExportGroup source, Local.GroupGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.DetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    useImport.Search.Flag = local.SearchOption.Flag;
    useImport.CsePersonsWorkSet.Ssn = export.Obligor.Ssn;

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Group, MoveExport1ToGroup);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Obligor.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Obligor.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCase()
  {
    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingObligor2.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.CseCases.IsFull)
        {
          return false;
        }

        entities.ExistingApCase.Number = db.GetString(reader, 0);
        entities.ExistingApCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingApCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.ExistingApCaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.ExistingObligor2.Number);
        db.SetNullableDate(command, "startDate", date);
      },
      (db, reader) =>
      {
        entities.ExistingApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingApCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCreditReporting()
  {
    entities.ExistingCred.Populated = false;

    return Read("ReadCreditReporting",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingObligor2.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCred.CpaType = db.GetString(reader, 0);
        entities.ExistingCred.CspNumber = db.GetString(reader, 1);
        entities.ExistingCred.Type1 = db.GetString(reader, 2);
        entities.ExistingCred.TakenDate = db.GetDate(reader, 3);
        entities.ExistingCred.OriginalAmount = db.GetNullableDecimal(reader, 4);
        entities.ExistingCred.CurrentAmount = db.GetNullableDecimal(reader, 5);
        entities.ExistingCred.NotificationDate = db.GetNullableDate(reader, 6);
        entities.ExistingCred.NotifiedBy = db.GetNullableString(reader, 7);
        entities.ExistingCred.ApRespReceivedData =
          db.GetNullableDate(reader, 8);
        entities.ExistingCred.DateStayed = db.GetNullableDate(reader, 9);
        entities.ExistingCred.DateStayReleased = db.GetNullableDate(reader, 10);
        entities.ExistingCred.HighestAmount = db.GetNullableDecimal(reader, 11);
        entities.ExistingCred.TanfCode = db.GetString(reader, 12);
        entities.ExistingCred.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingCred.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingCred.Type1);
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCred.Populated);

    return ReadEach("ReadCreditReportingAction",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.ExistingCred.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.ExistingCred.Type1);
        db.SetString(command, "aacTanfCode", entities.ExistingCred.TanfCode);
        db.SetString(command, "cspNumber", entities.ExistingCred.CspNumber);
        db.SetString(command, "cpaType", entities.ExistingCred.CpaType);
        db.SetNullableDate(
          command, "craTransDate1",
          local.CrRptActnBegDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "craTransDate2", local.EndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Creds.IsFull)
        {
          return false;
        }

        entities.ExistingCreditReportingAction.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingCreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.ExistingCreditReportingAction.CraTransCode =
          db.GetString(reader, 2);
        entities.ExistingCreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.ExistingCreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingCreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingCreditReportingAction.CpaType =
          db.GetString(reader, 8);
        entities.ExistingCreditReportingAction.CspNumber =
          db.GetString(reader, 9);
        entities.ExistingCreditReportingAction.AacType =
          db.GetString(reader, 10);
        entities.ExistingCreditReportingAction.AacTakenDate =
          db.GetDate(reader, 11);
        entities.ExistingCreditReportingAction.AacTanfCode =
          db.GetString(reader, 12);
        entities.ExistingCreditReportingAction.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.ExistingCreditReportingAction.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.ExistingCreditReportingAction.AacType);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingObligor2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligor2.Number = db.GetString(reader, 0);
        entities.ExistingObligor2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingApCaseRole.Populated);
    entities.GoodCause.Populated = false;

    return ReadEach("ReadGoodCause",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableString(
          command, "casNumber1", entities.ExistingApCaseRole.CasNumber);
        db.SetNullableInt32(
          command, "croIdentifier1", entities.ExistingApCaseRole.Identifier);
        db.SetNullableString(
          command, "croType1", entities.ExistingApCaseRole.Type1);
        db.SetNullableString(
          command, "cspNumber1", entities.ExistingApCaseRole.CspNumber);
        db.SetNullableDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);

        return true;
      });
  }

  private bool ReadObligationAdmActionExemption()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDt", date);
        db.SetString(command, "cspNumber", entities.ExistingObligor2.Number);
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
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
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public CreditReportingAction End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CreditReportingAction end;
    private CsePersonsWorkSet obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CredsGroup group.</summary>
    [Serializable]
    public class CredsGroup
    {
      /// <summary>
      /// A value of CredsDetail.
      /// </summary>
      [JsonPropertyName("credsDetail")]
      public CreditReportingAction CredsDetail
      {
        get => credsDetail ??= new();
        set => credsDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CreditReportingAction credsDetail;
    }

    /// <summary>
    /// A value of Export1St.
    /// </summary>
    [JsonPropertyName("export1St")]
    public Case1 Export1St
    {
      get => export1St ??= new();
      set => export1St = value;
    }

    /// <summary>
    /// A value of Export2Nd.
    /// </summary>
    [JsonPropertyName("export2Nd")]
    public Case1 Export2Nd
    {
      get => export2Nd ??= new();
      set => export2Nd = value;
    }

    /// <summary>
    /// A value of Export3Rd.
    /// </summary>
    [JsonPropertyName("export3Rd")]
    public Case1 Export3Rd
    {
      get => export3Rd ??= new();
      set => export3Rd = value;
    }

    /// <summary>
    /// A value of Export4Th.
    /// </summary>
    [JsonPropertyName("export4Th")]
    public Case1 Export4Th
    {
      get => export4Th ??= new();
      set => export4Th = value;
    }

    /// <summary>
    /// A value of Export5Th.
    /// </summary>
    [JsonPropertyName("export5Th")]
    public Case1 Export5Th
    {
      get => export5Th ??= new();
      set => export5Th = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of MoreCseCasesInd.
    /// </summary>
    [JsonPropertyName("moreCseCasesInd")]
    public Standard MoreCseCasesInd
    {
      get => moreCseCasesInd ??= new();
      set => moreCseCasesInd = value;
    }

    /// <summary>
    /// Gets a value of Creds.
    /// </summary>
    [JsonIgnore]
    public Array<CredsGroup> Creds => creds ??= new(CredsGroup.Capacity);

    /// <summary>
    /// Gets a value of Creds for json serialization.
    /// </summary>
    [JsonPropertyName("creds")]
    [Computed]
    public IList<CredsGroup> Creds_Json
    {
      get => creds;
      set => Creds.Assign(value);
    }

    /// <summary>
    /// A value of AdminExemptExists.
    /// </summary>
    [JsonPropertyName("adminExemptExists")]
    public Common AdminExemptExists
    {
      get => adminExemptExists ??= new();
      set => adminExemptExists = value;
    }

    /// <summary>
    /// A value of GoodCauseExists.
    /// </summary>
    [JsonPropertyName("goodCauseExists")]
    public Common GoodCauseExists
    {
      get => goodCauseExists ??= new();
      set => goodCauseExists = value;
    }

    private Case1 export1St;
    private Case1 export2Nd;
    private Case1 export3Rd;
    private Case1 export4Th;
    private Case1 export5Th;
    private CsePersonsWorkSet obligor;
    private AdministrativeActCertification creditReporting;
    private Standard moreCseCasesInd;
    private Array<CredsGroup> creds;
    private Common adminExemptExists;
    private Common goodCauseExists;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
    {
      /// <summary>
      /// A value of CseCasesDetail.
      /// </summary>
      [JsonPropertyName("cseCasesDetail")]
      public Case1 CseCasesDetail
      {
        get => cseCasesDetail ??= new();
        set => cseCasesDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Case1 cseCasesDetail;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
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
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of CrRptActnRecCnt.
    /// </summary>
    [JsonPropertyName("crRptActnRecCnt")]
    public Common CrRptActnRecCnt
    {
      get => crRptActnRecCnt ??= new();
      set => crRptActnRecCnt = value;
    }

    /// <summary>
    /// A value of CrRptActnBegDate.
    /// </summary>
    [JsonPropertyName("crRptActnBegDate")]
    public DateWorkArea CrRptActnBegDate
    {
      get => crRptActnBegDate ??= new();
      set => crRptActnBegDate = value;
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
    /// A value of BegDate.
    /// </summary>
    [JsonPropertyName("begDate")]
    public DateWorkArea BegDate
    {
      get => begDate ??= new();
      set => begDate = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of HighAmt.
    /// </summary>
    [JsonPropertyName("highAmt")]
    public Common HighAmt
    {
      get => highAmt ??= new();
      set => highAmt = value;
    }

    /// <summary>
    /// A value of OrigAmt.
    /// </summary>
    [JsonPropertyName("origAmt")]
    public Common OrigAmt
    {
      get => origAmt ??= new();
      set => origAmt = value;
    }

    /// <summary>
    /// A value of CurrentAmt.
    /// </summary>
    [JsonPropertyName("currentAmt")]
    public Common CurrentAmt
    {
      get => currentAmt ??= new();
      set => currentAmt = value;
    }

    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
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
    /// A value of CseCaseGroupIndex.
    /// </summary>
    [JsonPropertyName("cseCaseGroupIndex")]
    public Common CseCaseGroupIndex
    {
      get => cseCaseGroupIndex ??= new();
      set => cseCaseGroupIndex = value;
    }

    /// <summary>
    /// A value of NoOfCseCases.
    /// </summary>
    [JsonPropertyName("noOfCseCases")]
    public Common NoOfCseCases
    {
      get => noOfCseCases ??= new();
      set => noOfCseCases = value;
    }

    /// <summary>
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    /// <summary>
    /// A value of NoOfAdmActCertRecs.
    /// </summary>
    [JsonPropertyName("noOfAdmActCertRecs")]
    public Common NoOfAdmActCertRecs
    {
      get => noOfAdmActCertRecs ??= new();
      set => noOfAdmActCertRecs = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of SearchOption.
    /// </summary>
    [JsonPropertyName("searchOption")]
    public Common SearchOption
    {
      get => searchOption ??= new();
      set => searchOption = value;
    }

    private Common crRptActnRecCnt;
    private DateWorkArea crRptActnBegDate;
    private DateWorkArea nullDate;
    private DateWorkArea begDate;
    private DateWorkArea endDate;
    private Common highAmt;
    private Common origAmt;
    private Common currentAmt;
    private CreditReportingAction creditReportingAction;
    private DateWorkArea current;
    private Common cseCaseGroupIndex;
    private Common noOfCseCases;
    private Array<CseCasesGroup> cseCases;
    private Common noOfAdmActCertRecs;
    private CsePersonsWorkSet obligor;
    private Array<GroupGroup> group;
    private Common searchOption;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of ExistingApCaseRole.
    /// </summary>
    [JsonPropertyName("existingApCaseRole")]
    public CaseRole ExistingApCaseRole
    {
      get => existingApCaseRole ??= new();
      set => existingApCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingApCase.
    /// </summary>
    [JsonPropertyName("existingApCase")]
    public Case1 ExistingApCase
    {
      get => existingApCase ??= new();
      set => existingApCase = value;
    }

    /// <summary>
    /// A value of ExistingCreditReportingAction.
    /// </summary>
    [JsonPropertyName("existingCreditReportingAction")]
    public CreditReportingAction ExistingCreditReportingAction
    {
      get => existingCreditReportingAction ??= new();
      set => existingCreditReportingAction = value;
    }

    /// <summary>
    /// A value of ExistingCred.
    /// </summary>
    [JsonPropertyName("existingCred")]
    public AdministrativeActCertification ExistingCred
    {
      get => existingCred ??= new();
      set => existingCred = value;
    }

    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePersonAccount ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePerson ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    private Obligation obligation;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private GoodCause goodCause;
    private CaseRole existingApCaseRole;
    private Case1 existingApCase;
    private CreditReportingAction existingCreditReportingAction;
    private AdministrativeActCertification existingCred;
    private CsePersonAccount existingObligor1;
    private CsePerson existingObligor2;
  }
#endregion
}
