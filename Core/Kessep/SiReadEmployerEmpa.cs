// Program: SI_READ_EMPLOYER_EMPA, ID: 371766117, model: 746.
// Short name: SWE01219
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_READ_EMPLOYER_EMPA.
/// </summary>
[Serializable]
public partial class SiReadEmployerEmpa: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_EMPLOYER_EMPA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadEmployerEmpa(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadEmployerEmpa.
  /// </summary>
  public SiReadEmployerEmpa(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 10-24-95 H. Sharland     Initial Development
    // 02-04-95 P.Elie          Retrofit Security & Next Tran
    // 04-29-97 A.Kinney	Changed Current_Date
    // ---------------------------------------------
    // 07/13/99  Marek Lachowicz   Change property of READ (Select Only)
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (ReadEmployer())
    {
      export.WorksiteEmployer.Assign(entities.Ws);

      if (ReadEmployerAddress2())
      {
        export.WorksiteEmployerAddress.Assign(entities.EmployerAddress);
      }
      else
      {
        ExitState = "EMPLOYER_ADDRESS_NF";

        return;
      }
    }
    else
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    export.Empa.Index = -1;
    export.Empa.Count = 0;

    if (AsChar(import.ScreenSelect.Text1) == 'X')
    {
      // reverse read, from the provider point of view. all the worksites it 
      // provides services for
      foreach(var item in ReadEmployerRelationEmployer1())
      {
        if (!IsEmpty(import.EmployerRelation.Type1) && !
          Equal(entities.EmployerRelation.Type1, import.EmployerRelation.Type1))
        {
          continue;
        }

        ++export.Empa.Index;
        export.Empa.CheckSize();

        export.Empa.Update.Employer.Assign(entities.Provider);
        export.Empa.Update.OrginalEmployer.Identifier =
          entities.Provider.Identifier;
        export.Empa.Update.EmployerRelation.Assign(entities.EmployerRelation);
        export.Empa.Update.OrginalEmployerRelation.Type1 =
          entities.EmployerRelation.Type1;
        export.Empa.Update.EffDate.Date =
          entities.EmployerRelation.EffectiveDate;

        if (Equal(entities.EmployerRelation.EndDate, local.Max.Date))
        {
          export.Empa.Update.EndDate.Date = local.Null1.Date;
        }
        else
        {
          export.Empa.Update.EndDate.Date = entities.EmployerRelation.EndDate;
        }

        if (ReadEmployerAddress1())
        {
          export.Empa.Update.EmployerAddress.Assign(entities.EmployerAddress);
        }
        else
        {
          ExitState = "EMPLOYER_ADDRESS_NF";
        }

        if (export.Empa.IsFull)
        {
          break;
        }
      }
    }
    else
    {
      foreach(var item in ReadEmployerRelationEmployer2())
      {
        if (!IsEmpty(import.EmployerRelation.Type1) && !
          Equal(entities.EmployerRelation.Type1, import.EmployerRelation.Type1))
        {
          continue;
        }

        ++export.Empa.Index;
        export.Empa.CheckSize();

        export.Empa.Update.Employer.Assign(entities.Provider);
        export.Empa.Update.OrginalEmployer.Identifier =
          entities.Provider.Identifier;
        export.Empa.Update.EmployerRelation.Assign(entities.EmployerRelation);
        export.Empa.Update.OrginalEmployerRelation.Type1 =
          entities.EmployerRelation.Type1;
        export.Empa.Update.EffDate.Date =
          entities.EmployerRelation.EffectiveDate;

        if (Equal(entities.EmployerRelation.EndDate, local.Max.Date))
        {
          export.Empa.Update.EndDate.Date = local.Null1.Date;
        }
        else
        {
          export.Empa.Update.EndDate.Date = entities.EmployerRelation.EndDate;
        }

        if (ReadEmployerAddress1())
        {
          export.Empa.Update.EmployerAddress.Assign(entities.EmployerAddress);
        }
        else
        {
          ExitState = "EMPLOYER_ADDRESS_NF";
        }

        if (export.Empa.IsFull)
        {
          break;
        }
      }
    }

    if (export.Empa.IsEmpty)
    {
      ExitState = "EMPLOYER_RELATION_NF";
    }
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
    entities.Ws.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Ws.Identifier);
      },
      (db, reader) =>
      {
        entities.Ws.Identifier = db.GetInt32(reader, 0);
        entities.Ws.Ein = db.GetNullableString(reader, 1);
        entities.Ws.Name = db.GetNullableString(reader, 2);
        entities.Ws.Populated = true;
      });
  }

  private bool ReadEmployerAddress1()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress1",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Provider.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.State = db.GetNullableString(reader, 5);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 8);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadEmployerAddress2()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress2",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Ws.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.State = db.GetNullableString(reader, 5);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 8);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadEmployerRelationEmployer1()
  {
    entities.EmployerRelation.Populated = false;
    entities.Provider.Populated = false;

    return ReadEach("ReadEmployerRelationEmployer1",
      (db, command) =>
      {
        db.SetInt32(command, "empHqId", entities.Ws.Identifier);
        db.SetNullableDate(
          command, "endDate", import.Scroll.EndDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Scroll.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 4);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 5);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 6);
        entities.Provider.Identifier = db.GetInt32(reader, 6);
        entities.EmployerRelation.Note = db.GetNullableString(reader, 7);
        entities.EmployerRelation.Type1 = db.GetString(reader, 8);
        entities.Provider.Ein = db.GetNullableString(reader, 9);
        entities.Provider.Name = db.GetNullableString(reader, 10);
        entities.EmployerRelation.Populated = true;
        entities.Provider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadEmployerRelationEmployer2()
  {
    entities.EmployerRelation.Populated = false;
    entities.Provider.Populated = false;

    return ReadEach("ReadEmployerRelationEmployer2",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", entities.Ws.Identifier);
        db.SetNullableDate(
          command, "endDate", import.Scroll.EndDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Scroll.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 4);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 5);
        entities.Provider.Identifier = db.GetInt32(reader, 5);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 6);
        entities.EmployerRelation.Note = db.GetNullableString(reader, 7);
        entities.EmployerRelation.Type1 = db.GetString(reader, 8);
        entities.Provider.Ein = db.GetNullableString(reader, 9);
        entities.Provider.Name = db.GetNullableString(reader, 10);
        entities.EmployerRelation.Populated = true;
        entities.Provider.Populated = true;

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
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public EmployerRelation Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    /// <summary>
    /// A value of ScreenSelect.
    /// </summary>
    [JsonPropertyName("screenSelect")]
    public WorkArea ScreenSelect
    {
      get => screenSelect ??= new();
      set => screenSelect = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    private EmployerRelation scroll;
    private Employer ws;
    private WorkArea screenSelect;
    private EmployerRelation employerRelation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A EmpaGroup group.</summary>
    [Serializable]
    public class EmpaGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

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
      /// A value of PrmpEmpl.
      /// </summary>
      [JsonPropertyName("prmpEmpl")]
      public Common PrmpEmpl
      {
        get => prmpEmpl ??= new();
        set => prmpEmpl = value;
      }

      /// <summary>
      /// A value of PrmpType.
      /// </summary>
      [JsonPropertyName("prmpType")]
      public Common PrmpType
      {
        get => prmpType ??= new();
        set => prmpType = value;
      }

      /// <summary>
      /// A value of EmployerRelation.
      /// </summary>
      [JsonPropertyName("employerRelation")]
      public EmployerRelation EmployerRelation
      {
        get => employerRelation ??= new();
        set => employerRelation = value;
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
      /// A value of EffDate.
      /// </summary>
      [JsonPropertyName("effDate")]
      public DateWorkArea EffDate
      {
        get => effDate ??= new();
        set => effDate = value;
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
      /// A value of OrginalEmployerRelation.
      /// </summary>
      [JsonPropertyName("orginalEmployerRelation")]
      public EmployerRelation OrginalEmployerRelation
      {
        get => orginalEmployerRelation ??= new();
        set => orginalEmployerRelation = value;
      }

      /// <summary>
      /// A value of OrginalEmployer.
      /// </summary>
      [JsonPropertyName("orginalEmployer")]
      public Employer OrginalEmployer
      {
        get => orginalEmployer ??= new();
        set => orginalEmployer = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common select;
      private Employer employer;
      private Common prmpEmpl;
      private Common prmpType;
      private EmployerRelation employerRelation;
      private EmployerAddress employerAddress;
      private DateWorkArea effDate;
      private DateWorkArea endDate;
      private EmployerRelation orginalEmployerRelation;
      private Employer orginalEmployer;
    }

    /// <summary>
    /// A value of WorksiteEmployerAddress.
    /// </summary>
    [JsonPropertyName("worksiteEmployerAddress")]
    public EmployerAddress WorksiteEmployerAddress
    {
      get => worksiteEmployerAddress ??= new();
      set => worksiteEmployerAddress = value;
    }

    /// <summary>
    /// A value of WorksiteEmployer.
    /// </summary>
    [JsonPropertyName("worksiteEmployer")]
    public Employer WorksiteEmployer
    {
      get => worksiteEmployer ??= new();
      set => worksiteEmployer = value;
    }

    /// <summary>
    /// Gets a value of Empa.
    /// </summary>
    [JsonIgnore]
    public Array<EmpaGroup> Empa => empa ??= new(EmpaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Empa for json serialization.
    /// </summary>
    [JsonPropertyName("empa")]
    [Computed]
    public IList<EmpaGroup> Empa_Json
    {
      get => empa;
      set => Empa.Assign(value);
    }

    private EmployerAddress worksiteEmployerAddress;
    private Employer worksiteEmployer;
    private Array<EmpaGroup> empa;
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

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private DateWorkArea max;
    private DateWorkArea null1;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of Provider.
    /// </summary>
    [JsonPropertyName("provider")]
    public Employer Provider
    {
      get => provider ??= new();
      set => provider = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    private EmployerAddress employerAddress;
    private EmployerRelation employerRelation;
    private Employer provider;
    private Employer ws;
  }
#endregion
}
