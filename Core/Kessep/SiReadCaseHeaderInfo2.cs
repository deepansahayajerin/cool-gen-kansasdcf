// Program: SI_READ_CASE_HEADER_INFO_2, ID: 1902634484, model: 746.
// Short name: SWE01251
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
/// A program: SI_READ_CASE_HEADER_INFO_2.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This process reads the AP and AR for a given case
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCaseHeaderInfo2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CASE_HEADER_INFO_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCaseHeaderInfo2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCaseHeaderInfo2.
  /// </summary>
  public SiReadCaseHeaderInfo2(IContext context, Import import, Export export):
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
    // 		M A I N T E N A N C E   L O G
    // Date		Developer		Request #	Description
    // 01-10-2018	Dwayne Dupree		58691	        Initial Dev
    // -------------------------------------------------------------------
    local.ErrOnAdabasUnavailable.Flag = "Y";
    export.CaseOpen.Flag = "";
    export.ApActive.Flag = "";
    local.Current.Date = Now().Date;

    if (ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'O')
      {
        export.CaseOpen.Flag = "Y";
      }
      else if (AsChar(entities.Case1.Status) == 'C')
      {
        export.CaseOpen.Flag = "N";
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------
    // Read AR for the OPEN case
    // ---------------------------------------------
    if (AsChar(export.CaseOpen.Flag) == 'Y')
    {
      if (ReadCsePersonCaseRole5())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        if (!Equal(entities.CaseRole.StartDate, local.Current.Date) && !
          Lt(entities.CaseRole.EndDate, local.Current.Date))
        {
          export.ArActive.Flag = "Y";
        }

        if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
        {
          UseCabReadAdabasPerson1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseSiFormatCsePersonName1();
          export.Ar.FormattedName = local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          export.Ar.Number = entities.CsePerson.Number;
          export.Ar.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
            (33);
        }

        goto Test1;
      }

      ExitState = "AR_DB_ERROR_NF";
    }

Test1:

    // ---------------------------------------------
    // Read AP for the OPEN case
    // ---------------------------------------------
    if (AsChar(export.CaseOpen.Flag) == 'Y')
    {
      if (IsEmpty(import.Ap.Number))
      {
        // ---------------------------------------------
        // Check to see how many active APs are on a
        // case.  If only one, read that one else set an
        // indicator to make the procedure flow to Case
        // Composition.
        // ---------------------------------------------
        local.Common.Count = 0;

        foreach(var item in ReadCsePersonCaseRole7())
        {
          ++local.Common.Count;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "NO_APS_ON_A_CASE";

            return;
          case 1:
            if (ReadCsePersonCaseRole1())
            {
              local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

              if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
              {
                UseCabReadAdabasPerson2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                UseSiFormatCsePersonName2();
                export.Ap.FormattedName = local.CsePersonsWorkSet.FormattedName;
              }
              else
              {
                export.Ap.FormattedName =
                  entities.CsePerson.OrganizationName ?? Spaces(33);
                export.Ap.Number = entities.CsePerson.Number;
              }
            }
            else
            {
              // ---------------------------------------------
              // No APs for this case
              // ---------------------------------------------
              return;
            }

            break;
          default:
            export.MultipleAps.Flag = "Y";

            return;
        }
      }
      else
      {
        if (ReadCsePersonCaseRole4())
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

          if (!Lt(local.Current.Date, entities.CaseRole.StartDate) && !
            Lt(entities.CaseRole.EndDate, local.Current.Date))
          {
            export.ApActive.Flag = "Y";
          }
          else
          {
            export.ApActive.Flag = "N";
          }

          if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
          {
            UseCabReadAdabasPerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            UseSiFormatCsePersonName2();
            export.Ap.FormattedName = local.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Ap.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
              (33);
            export.Ap.Number = entities.CsePerson.Number;
          }

          goto Test2;
        }

        ExitState = "INVALID_CASE_ROLE";

        return;
      }
    }

Test2:

    // ---------------------------------------------
    // Read AR for the CLOSED case
    // ---------------------------------------------
    if (AsChar(export.CaseOpen.Flag) == 'N')
    {
      if (ReadCsePersonCaseRole5())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
        {
          UseCabReadAdabasPerson1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseSiFormatCsePersonName1();
          export.Ar.FormattedName = local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          export.Ar.Number = entities.CsePerson.Number;
          export.Ar.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
            (33);
        }

        export.ArActive.Flag = "N";

        goto Test3;
      }

      if (!entities.CaseRole.Populated)
      {
        ExitState = "AR_DB_ERROR_NF";
      }
    }

Test3:

    // ---------------------------------------------
    // Read AP for the CLOSED case
    // ---------------------------------------------
    if (AsChar(export.CaseOpen.Flag) == 'N')
    {
      if (IsEmpty(import.Ap.Number))
      {
        // ---------------------------------------------
        // Check to see how many APs are on a case.  If
        // only one, read that one else set an indicator
        // to make the procedure flow to Case Composition.
        // ---------------------------------------------
        local.Common.Count = 0;

        // 10-03-00 M.L Read APs active on case closure date.
        foreach(var item in ReadCsePersonCaseRole6())
        {
          ++local.Common.Count;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "NO_APS_ON_A_CASE";

            break;
          case 1:
            if (ReadCsePersonCaseRole3())
            {
              if (!Lt(local.Current.Date, entities.CaseRole.StartDate) && !
                Lt(entities.CaseRole.EndDate, local.Current.Date))
              {
                export.ApActive.Flag = "Y";
              }
              else
              {
                export.ApActive.Flag = "N";
              }

              local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

              if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
              {
                UseCabReadAdabasPerson2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                UseSiFormatCsePersonName2();
                export.Ap.FormattedName = local.CsePersonsWorkSet.FormattedName;
              }
              else
              {
                export.Ap.FormattedName =
                  entities.CsePerson.OrganizationName ?? Spaces(33);
                export.Ap.Number = entities.CsePerson.Number;
              }
            }
            else
            {
              // ---------------------------------------------
              // No APs for this case
              // ---------------------------------------------
            }

            break;
          default:
            export.MultipleAps.Flag = "Y";

            break;
        }
      }
      else if (ReadCsePersonCaseRole2())
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        if (!Lt(local.Current.Date, entities.CaseRole.StartDate) && !
          Lt(entities.CaseRole.EndDate, local.Current.Date))
        {
          export.ApActive.Flag = "Y";
        }
        else
        {
          export.ApActive.Flag = "N";
        }

        if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
        {
          UseCabReadAdabasPerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseSiFormatCsePersonName2();
          export.Ap.FormattedName = local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          export.Ap.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
            (33);
          export.Ap.Number = entities.CsePerson.Number;
        }
      }
      else
      {
        ExitState = "INVALID_CASE_ROLE";
      }
    }
  }

  private void UseCabReadAdabasPerson1()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    export.Ae.Flag = useExport.Ae.Flag;
    export.AbendData.Assign(useExport.AbendData);
    export.Ar.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabReadAdabasPerson2()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    export.Ae.Flag = useExport.Ae.Flag;
    export.AbendData.Assign(useExport.AbendData);
    export.Ap.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.Ar);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.Ap);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
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
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole3()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole4()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole4",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole5()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole5",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole6()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", entities.Case1.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole7()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole7",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
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

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private CsePersonsWorkSet ap;
    private Case1 case1;
    private CsePersonsWorkSet ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ArActive.
    /// </summary>
    [JsonPropertyName("arActive")]
    public Common ArActive
    {
      get => arActive ??= new();
      set => arActive = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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

    private Common arActive;
    private Common apActive;
    private Common caseOpen;
    private Common multipleAps;
    private Common ae;
    private AbendData abendData;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea current;
    private Common errOnAdabasUnavailable;
    private Common common;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
