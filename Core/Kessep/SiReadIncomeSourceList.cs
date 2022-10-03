// Program: SI_READ_INCOME_SOURCE_LIST, ID: 371762760, model: 746.
// Short name: SWE01223
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
/// A program: SI_READ_INCOME_SOURCE_LIST.
/// </para>
/// <para>
/// RESP: SRVINIT	
/// This AB reads all the income sources for a given person
/// </para>
/// </summary>
[Serializable]
public partial class SiReadIncomeSourceList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_INCOME_SOURCE_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadIncomeSourceList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadIncomeSourceList.
  /// </summary>
  public SiReadIncomeSourceList(IContext context, Import import, Export export):
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
    // 09-13-95	Helen Sharland - MTW		0	Initial Dev
    // 05-13-97	Sid		Conversion Fixes.
    // -------------------------------------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    UseCabSetMaximumDiscontinueDate();
    export.Export1.Index = -1;

    if (AsChar(import.Search.Type1) == 'A' || IsEmpty(import.Search.Type1))
    {
      foreach(var item in ReadIncomeSource1())
      {
        if (Equal(entities.IncomeSource.EndDt, import.Page.EndDt))
        {
          if (AsChar(entities.IncomeSource.Type1) == AsChar(import.Page.Type1))
          {
            if (Equal(entities.IncomeSource.ReturnDt, import.Page.ReturnDt))
            {
              if (Lt(entities.IncomeSource.Identifier, import.Page.Identifier))
              {
                continue;
              }
            }
            else if (Lt(import.Page.ReturnDt, entities.IncomeSource.ReturnDt))
            {
              continue;
            }
          }
          else if (AsChar(entities.IncomeSource.Type1) < AsChar
            (import.Page.Type1))
          {
            continue;
          }
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          export.Page.Identifier = entities.IncomeSource.Identifier;
          export.Page.Type1 = entities.IncomeSource.Type1;
          export.Page.EndDt = entities.IncomeSource.EndDt;
          export.Page.ReturnDt = entities.IncomeSource.ReturnDt;

          break;
        }

        export.Export1.Update.DetailIncomeSource.Assign(entities.IncomeSource);

        if (Equal(export.Export1.Item.DetailIncomeSource.EndDt, local.Max.Date))
        {
          export.Export1.Update.DetailIncomeSource.EndDt = null;
        }

        // -----------------------
        // Get most recent income.
        // -----------------------
        // 07/12/99  Changed property of READ EACH to OPTIMIZE FOR 1 ROW
        if (ReadPersonIncomeHistory())
        {
          UseCabComputeAvgMonthlyIncome();
        }

        // -----------------------
        // Get Employer details.
        // -----------------------
        // 07/12/99  Changed property of READ (Select Only)
        if (ReadEmployer())
        {
          export.Export1.Update.DetailIncomeSource.Name =
            entities.Employer.Name;

          if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
            Lt(entities.Employer.EiwoEndDate, Now().Date))
          {
            export.Export1.Update.Eiwo.Flag = "Y";
          }
          else
          {
            export.Export1.Update.Eiwo.Flag = "N";
          }
        }
        else
        {
          export.Export1.Update.Eiwo.Flag = "N";
        }
      }
    }
    else
    {
      foreach(var item in ReadIncomeSource2())
      {
        if (Equal(entities.IncomeSource.EndDt, import.Page.EndDt))
        {
          if (Equal(entities.IncomeSource.ReturnDt, import.Page.ReturnDt))
          {
            if (Lt(entities.IncomeSource.Identifier, import.Page.Identifier))
            {
              continue;
            }
          }
          else if (Lt(import.Page.ReturnDt, entities.IncomeSource.ReturnDt))
          {
            continue;
          }
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          export.Page.Identifier = entities.IncomeSource.Identifier;
          export.Page.Type1 = entities.IncomeSource.Type1;
          export.Page.EndDt = entities.IncomeSource.EndDt;
          export.Page.ReturnDt = entities.IncomeSource.ReturnDt;

          break;
        }

        export.Export1.Update.DetailIncomeSource.Assign(entities.IncomeSource);

        if (Equal(export.Export1.Item.DetailIncomeSource.EndDt, local.Max.Date))
        {
          export.Export1.Update.DetailIncomeSource.EndDt = null;
        }

        // -----------------------
        // Get most recent income.
        // -----------------------
        // 07/12/99  Changed property of READ EACH to OPTIMIZE FOR 1 ROW
        if (ReadPersonIncomeHistory())
        {
          UseCabComputeAvgMonthlyIncome();
        }

        // -----------------------
        // Get Employer details.
        // -----------------------
        // 07/12/99  Changed property of READ (Select Only)
        if (ReadEmployer())
        {
          export.Export1.Update.DetailIncomeSource.Name =
            entities.Employer.Name;

          if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
            Lt(entities.Employer.EiwoEndDate, Now().Date))
          {
            export.Export1.Update.Eiwo.Flag = "Y";
          }
          else
          {
            export.Export1.Update.Eiwo.Flag = "N";
          }
        }
        else
        {
          export.Export1.Update.Eiwo.Flag = "N";
        }
      }
    }

    if (import.Standard.PageNumber == 1)
    {
      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "";
      }
    }
    else if (export.Export1.Index >= Export.ExportGroup.Capacity)
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private void UseCabComputeAvgMonthlyIncome()
  {
    var useImport = new CabComputeAvgMonthlyIncome.Import();
    var useExport = new CabComputeAvgMonthlyIncome.Export();

    useImport.New1.Assign(entities.PersonIncomeHistory);

    Call(CabComputeAvgMonthlyIncome.Execute, useImport, useExport);

    export.Export1.Update.DetailMnthlyIncm.TotalCurrency =
      useExport.Common.TotalCurrency;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadEmployer()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 2);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 3);
        entities.Employer.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDt", import.Page.EndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 6);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 7);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource2()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDt", import.Page.EndDt.GetValueOrDefault());
        db.SetString(command, "type", import.Search.Type1);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 6);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 7);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

        return true;
      });
  }

  private bool ReadPersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.PersonIncomeHistory.Populated = false;

    return Read("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonIncomeHistory.CspNumber = db.GetString(reader, 0);
        entities.PersonIncomeHistory.IsrIdentifier = db.GetDateTime(reader, 1);
        entities.PersonIncomeHistory.Identifier = db.GetDateTime(reader, 2);
        entities.PersonIncomeHistory.IncomeEffDt =
          db.GetNullableDate(reader, 3);
        entities.PersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 4);
        entities.PersonIncomeHistory.Freq = db.GetNullableString(reader, 5);
        entities.PersonIncomeHistory.CspINumber = db.GetString(reader, 6);
        entities.PersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 7);
        entities.PersonIncomeHistory.Populated = true;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public IncomeSource Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public IncomeSource Page
    {
      get => page ??= new();
      set => page = value;
    }

    private IncomeSource search;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private IncomeSource page;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of Eiwo.
      /// </summary>
      [JsonPropertyName("eiwo")]
      public Common Eiwo
      {
        get => eiwo ??= new();
        set => eiwo = value;
      }

      /// <summary>
      /// A value of DetailIncomeSource.
      /// </summary>
      [JsonPropertyName("detailIncomeSource")]
      public IncomeSource DetailIncomeSource
      {
        get => detailIncomeSource ??= new();
        set => detailIncomeSource = value;
      }

      /// <summary>
      /// A value of DetailMnthlyIncm.
      /// </summary>
      [JsonPropertyName("detailMnthlyIncm")]
      public Common DetailMnthlyIncm
      {
        get => detailMnthlyIncm ??= new();
        set => detailMnthlyIncm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common detailCommon;
      private Common eiwo;
      private IncomeSource detailIncomeSource;
      private Common detailMnthlyIncm;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public IncomeSource Page
    {
      get => page ??= new();
      set => page = value;
    }

    private Array<ExportGroup> export1;
    private Standard standard;
    private IncomeSource page;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private DateWorkArea max;
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
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
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

    private Employer employer;
    private PersonIncomeHistory personIncomeHistory;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
  }
#endregion
}
