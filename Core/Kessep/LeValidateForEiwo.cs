// Program: LE_VALIDATE_FOR_EIWO, ID: 1902464863, model: 746.
// Short name: SWE00837
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_VALIDATE_FOR_EIWO.
/// </summary>
[Serializable]
public partial class LeValidateForEiwo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_VALIDATE_FOR_EIWO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeValidateForEiwo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeValidateForEiwo.
  /// </summary>
  public LeValidateForEiwo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 06/09/15  GVandy	CQ22212		Initial Code
    // 11/17/15  GVandy	CQ50342		Allow future start dates on WA, WC, and WL
    // 					legal details and their associated LOPS
    // 					entries.
    // 2/28/17   AHockman      CQ47796         change to allow IWOMODO to have 
    // zeroes in WC
    //                                         
    // and WA
    // -------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (AsChar(entities.LegalAction.Classification) == 'I' && (
      Equal(entities.LegalAction.ActionTaken, "IWO") || Equal
      (entities.LegalAction.ActionTaken, "IWOMODO") || Equal
      (entities.LegalAction.ActionTaken, "IWOTERM") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWOLS") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWOPT")))
    {
    }
    else
    {
      return;
    }

    // -----------------------------------------------------------------------------------
    // -- Service Provider Validation
    // -----------------------------------------------------------------------------------
    // -- Validate Service Provider for all eIWO actions
    if (!ReadLegalActionAssigment())
    {
      ExitState = "LE0000_EIWO_NO_SERVICE_PROVIDER";

      return;
    }

    // -----------------------------------------------------------------------------------
    // -- Legal Role Validation
    // -----------------------------------------------------------------------------------
    // -- Validate Legal Role types P, R, and C exist for all eIWO actions.
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.LegalActionPerson.Role = "P";

          break;
        case 2:
          local.LegalActionPerson.Role = "R";

          break;
        case 3:
          local.LegalActionPerson.Role = "C";

          break;
        default:
          break;
      }

      if (!ReadLegalActionPerson1())
      {
        ExitState = "LE0000_EIWO_LEGAL_ROLES_MISSING";

        return;
      }
    }

    // -----------------------------------------------------------------------------------
    // -- Legal Detail and Legal Obligation Validation
    // -----------------------------------------------------------------------------------
    // -- Validate Legal Details and Legal Obligations for IWO, IWOMODO, 
    // ORDIWO2, and ORDIWOLS actions
    if (Equal(import.LegalAction.ActionTaken, "IWO") || Equal
      (import.LegalAction.ActionTaken, "IWOMODO") || Equal
      (import.LegalAction.ActionTaken, "ORDIWO2"))
    {
      local.LegalDetail.Count = 0;

      foreach(var item in ReadLegalActionDetailObligationType2())
      {
        ++local.LegalDetail.Count;

        switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
        {
          case "":
            ExitState = "LE0000_EIWO_FREQ_OR_DOM_MISSING";

            break;
          case "M":
            if (Equal(entities.LegalActionDetail.DayOfMonth1, 0))
            {
              ExitState = "LE0000_EIWO_FREQ_OR_DOM_MISSING";
            }

            break;
          case "SM":
            if (Equal(entities.LegalActionDetail.DayOfMonth1, 0) || Equal
              (entities.LegalActionDetail.DayOfMonth2, 0))
            {
              ExitState = "LE0000_EIWO_FREQ_OR_DOM_MISSING";
            }

            break;
          case "W":
            if (Equal(entities.LegalActionDetail.DayOfWeek, 0))
            {
              ExitState = "LE0000_EIWO_FREQ_OR_DOM_MISSING";
            }

            break;
          case "BW":
            if (Equal(entities.LegalActionDetail.DayOfWeek, 0))
            {
              ExitState = "LE0000_EIWO_FREQ_OR_DOM_MISSING";
            }

            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ** 2/28/17  AHockman   cq47796   changed this to a case to allow 
        // processing to stay  the same for IWO
        //                                  
        // and ORDIWO2 but allow IWOMODO to
        // process on if WC and/or WA are
        // zero.
        switch(TrimEnd(import.LegalAction.ActionTaken))
        {
          case "IWO":
            if (Equal(entities.ObligationType.Code, "WC") && Equal
              (entities.LegalActionDetail.CurrentAmount, 0))
            {
              ExitState = "LE0000_EIWO_CURRENT_AMT_MISSING";

              return;
            }

            if (Equal(entities.ObligationType.Code, "WA") && Equal
              (entities.LegalActionDetail.ArrearsAmount, 0))
            {
              ExitState = "LE0000_EIWO_ARREARS_AMT_MISSING";

              return;
            }

            break;
          case "ORDIWO2":
            if (Equal(entities.ObligationType.Code, "WC") && Equal
              (entities.LegalActionDetail.CurrentAmount, 0))
            {
              ExitState = "LE0000_EIWO_CURRENT_AMT_MISSING";

              return;
            }

            if (Equal(entities.ObligationType.Code, "WA") && Equal
              (entities.LegalActionDetail.ArrearsAmount, 0))
            {
              ExitState = "LE0000_EIWO_ARREARS_AMT_MISSING";

              return;
            }

            break;
          default:
            break;
        }

        for(local.Common.Count = 1; local.Common.Count <= 2; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.LegalActionPerson.AccountType = "R";

              break;
            case 2:
              local.LegalActionPerson.AccountType = "S";

              break;
            default:
              break;
          }

          if (!ReadLegalActionPerson2())
          {
            ExitState = "LE0000_EIWO_LOPS_MISSING";

            return;
          }
        }
      }

      if (local.LegalDetail.Count == 0)
      {
        ExitState = "LE0000_WA_WC_MISSING";

        return;
      }
    }
    else if (Equal(import.LegalAction.ActionTaken, "ORDIWOLS"))
    {
      local.LegalDetail.Count = 0;

      foreach(var item in ReadLegalActionDetailObligationType1())
      {
        ++local.LegalDetail.Count;

        if (Equal(entities.LegalActionDetail.ArrearsAmount, 0))
        {
          ExitState = "LE0000_EIWO_ARREARS_AMT_MISSING";

          return;
        }

        for(local.Common.Count = 1; local.Common.Count <= 2; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.LegalActionPerson.AccountType = "R";

              break;
            case 2:
              local.LegalActionPerson.AccountType = "S";

              break;
            default:
              break;
          }

          if (!ReadLegalActionPerson2())
          {
            ExitState = "LE0000_EIWO_LOPS_MISSING";

            return;
          }
        }
      }

      if (local.LegalDetail.Count == 0)
      {
        ExitState = "LE0000_WL_MISSING";

        return;
      }
    }

    // -----------------------------------------------------------------------------------
    // -- AP Validation
    // -----------------------------------------------------------------------------------
    if (!IsEmpty(import.CsePerson.Number))
    {
      // -- User selected an AP/Case from LROL.  Validate that selection.
      // -- Selected role type must be "AP"
      if (!Equal(import.CaseRole.Type1, "AP"))
      {
        ExitState = "LE0000_EIWO_INVALID_ROLE_SELECT";

        return;
      }

      if (Equal(import.CaseRole.Type1, "AP"))
      {
        // -- Selected "AP" must have an active LROL record.
        if (ReadCsePersonCase2())
        {
          goto Test1;
        }

        ExitState = "LE0000_EIWO_INVALID_AP_ROLE";

        return;
      }

Test1:

      if (Equal(entities.LegalAction.ActionTaken, "IWO") || Equal
        (entities.LegalAction.ActionTaken, "IWOMODO") || Equal
        (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
        (entities.LegalAction.ActionTaken, "ORDIWOLS"))
      {
        // -- Selected "AP" must have an active Obligor LOPS record.
        if (ReadCsePersonCase1())
        {
          goto Test2;
        }

        ExitState = "LE0000_EIWO_MISSING_OBLIGOR_LOPS";

        return;
      }

Test2:

      export.CsePersonsWorkSet.Number = import.CsePerson.Number;
      export.Case1.Number = import.Case1.Number;
    }
    else
    {
      // -- Determine if user selection of AP/Case from LROL is necessary.
      // --  If more than one AP/case combination is active on LROL then flow to
      // LROL for selection.
      local.Common.Count = 0;

      foreach(var item in ReadCsePersonCase4())
      {
        ++local.Common.Count;
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        export.Case1.Number = entities.Case1.Number;

        if (local.Common.Count > 1)
        {
          break;
        }
      }

      foreach(var item in ReadCsePersonCase3())
      {
        if (Equal(export.Case1.Number, entities.Case1.Number) && Equal
          (export.CsePersonsWorkSet.Number, entities.CsePerson.Number))
        {
          continue;
        }

        ++local.Common.Count;
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        export.Case1.Number = entities.Case1.Number;

        if (local.Common.Count > 1)
        {
          break;
        }
      }

      switch(local.Common.Count)
      {
        case 0:
          break;
        case 1:
          break;
        default:
          ExitState = "ECO_LNK_TO_LROL";

          return;
      }
    }

    // -- All edits were sucessful.
    if (Equal(global.TranCode, "SR9L"))
    {
      // -- If the cab is called from LACT then set an exit state to flow to 
      // LAIS.
      ExitState = "ECO_LNK_TO_LAIS";
    }
    else
    {
      // -- Continue.
    }
  }

  private bool ReadCsePersonCase1()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
        db.SetString(command, "casNum", import.Case1.Number);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonCase2()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
        db.SetString(command, "casNum", import.Case1.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCase3()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonCase3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCase4()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonCase4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;

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
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 3);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType1()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
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
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType2()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
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
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetString(command, "role", local.LegalActionPerson.Role);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "accountType", local.LegalActionPerson.AccountType ?? "");
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LegalDetail.
    /// </summary>
    [JsonPropertyName("legalDetail")]
    public Common LegalDetail
    {
      get => legalDetail ??= new();
      set => legalDetail = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private Common legalDetail;
    private LegalActionPerson legalActionPerson;
    private Common common;
    private DateWorkArea current;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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

    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
  }
#endregion
}
