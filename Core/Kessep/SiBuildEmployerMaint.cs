// Program: SI_BUILD_EMPLOYER_MAINT, ID: 371762203, model: 746.
// Short name: SWE01104
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
/// A program: SI_BUILD_EMPLOYER_MAINT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiBuildEmployerMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_EMPLOYER_MAINT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildEmployerMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildEmployerMaint.
  /// </summary>
  public SiBuildEmployerMaint(IContext context, Import import, Export export):
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
    // 09-24-95 K Evans     Initial Development
    // 10-20-99 D Lowry     Problem to fix performance
    // 01-13-2000  Sree Veettil   Made changes in the reads as
    // diplayed records were not in sorted order.
    // ---------------------------------------------
    // 09-20-00 P Phinney   H00103803 - Not selecting properly if name = 33 
    // chanracters
    // ---------------------------------------------
    // -- WR 10355 changes by L Bachura 10-05-01. These changes install the EIN 
    // search both with and without a wildcard. The wildcard search was also
    // added for the city. Likewise, search for a blank state was also added.
    // Changed 10-22-01 @ 1015
    // 06-27-2002 M Lachowicz  PR00146849 - Fixed scrolling problem.
    // 01/15/2016 D Dupree  pr50578  - Rewrote cab to simplify and fix scrolling
    // and data retriveal problems
    export.Group.Index = -1;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.CurentDate.Date = Now().Date;
    local.Local30Day.Date = AddDays(local.CurentDate.Date, -30);
    UseCabDate2TextWithHyphens();
    local.Local30Day.Timestamp = Timestamp(local.TextDate.Text10);
    local.SearchEmployer.Name = TrimEnd(import.SearchEmployer.Name) + "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"
      ;
    local.SearchEmployer.Ein = TrimEnd(import.SearchEmployer.Ein) + "%%%%%%%%%%%"
      ;
    local.SearchEmployerAddress.City =
      TrimEnd(import.SearchEmployerAddress.City) + "%%%%%%%%%%%%%%%%%%";
    local.SearchEmployerAddress.State =
      TrimEnd(import.SearchEmployerAddress.State) + "%%";
    local.SearchEmployerAddress.ZipCode =
      TrimEnd(import.SearchEmployerAddress.ZipCode) + "%%%%%";

    foreach(var item in ReadEmployerEmployerAddress())
    {
      if (AsChar(import.IncludeInactive.Flag) != 'Y')
      {
        if (Lt(entities.Employer.EndDate, local.CurentDate.Date))
        {
          continue;
        }

        if (!ReadIncomeSource())
        {
          if (!ReadEmployerRelation())
          {
            if (Lt(local.Local30Day.Date, entities.Employer.EffectiveDate))
            {
            }
            else
            {
              continue;
            }
          }
        }
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.GdetailEmployer.Assign(entities.Employer);

      if (Equal(entities.Employer.EiwoEndDate, local.Max.Date))
      {
        export.Group.Update.GeiwoEnd.Date = local.Null1.Date;
      }
      else
      {
        export.Group.Update.GeiwoEnd.Date = entities.Employer.EiwoEndDate;
      }

      export.Group.Update.GdetailEmployerAddress.
        Assign(entities.EmployerAddress);
      export.Group.Update.GcreateDate.Date =
        Date(entities.Employer.CreatedTstamp);

      if (Equal(entities.Employer.EndDate, local.Max.Date))
      {
      }
      else
      {
        export.Group.Update.Gend.Date = entities.Employer.EndDate;
      }

      if (export.Group.IsFull)
      {
        return;
      }
    }
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Local30Day.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private IEnumerable<bool> ReadEmployerEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return ReadEach("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "name1", local.SearchEmployer.Name ?? "");
        db.SetNullableString(command, "ein", local.SearchEmployer.Ein ?? "");
        db.SetNullableString(
          command, "city", local.SearchEmployerAddress.City ?? "");
        db.SetNullableString(
          command, "zipCode1", local.SearchEmployerAddress.ZipCode ?? "");
        db.SetNullableString(
          command, "state1", import.SearchEmployerAddress.State ?? "");
        db.SetNullableString(
          command, "state2", local.SearchEmployerAddress.State ?? "");
        db.
          SetNullableString(command, "name2", import.ScrollEmployer.Name ?? "");
          
        db.SetInt32(command, "identifier1", import.ScrollEmployer.Identifier);
        db.SetNullableString(
          command, "zipCode2", import.ScrollEmployerAddress.ZipCode ?? "");
        db.SetDateTime(
          command, "identifier2",
          import.ScrollEmployerAddress.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 5);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 6);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 7);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 8);
        entities.Employer.FaxAreaCode = db.GetNullableInt32(reader, 9);
        entities.Employer.FaxPhoneNo = db.GetNullableString(reader, 10);
        entities.Employer.EmailAddress = db.GetNullableString(reader, 11);
        entities.Employer.EffectiveDate = db.GetNullableDate(reader, 12);
        entities.Employer.EndDate = db.GetNullableDate(reader, 13);
        entities.EmployerAddress.LocationType = db.GetString(reader, 14);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 15);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 16);
        entities.EmployerAddress.City = db.GetNullableString(reader, 17);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 18);
        entities.EmployerAddress.State = db.GetNullableString(reader, 19);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 20);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 21);
        entities.EmployerAddress.Note = db.GetNullableString(reader, 22);
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);

        return true;
      });
  }

  private bool ReadEmployerRelation()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation",
      (db, command) =>
      {
        db.SetInt32(command, "empHqId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", local.CurentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 3);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 4);
        entities.EmployerRelation.Type1 = db.GetString(reader, 5);
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "endDt", local.CurentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 2);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 4);
        entities.IncomeSource.Populated = true;
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
    /// A value of IncludeInactive.
    /// </summary>
    [JsonPropertyName("includeInactive")]
    public Common IncludeInactive
    {
      get => includeInactive ??= new();
      set => includeInactive = value;
    }

    /// <summary>
    /// A value of SearchEmployer.
    /// </summary>
    [JsonPropertyName("searchEmployer")]
    public Employer SearchEmployer
    {
      get => searchEmployer ??= new();
      set => searchEmployer = value;
    }

    /// <summary>
    /// A value of SearchEmployerAddress.
    /// </summary>
    [JsonPropertyName("searchEmployerAddress")]
    public EmployerAddress SearchEmployerAddress
    {
      get => searchEmployerAddress ??= new();
      set => searchEmployerAddress = value;
    }

    /// <summary>
    /// A value of ScrollEmployer.
    /// </summary>
    [JsonPropertyName("scrollEmployer")]
    public Employer ScrollEmployer
    {
      get => scrollEmployer ??= new();
      set => scrollEmployer = value;
    }

    /// <summary>
    /// A value of ScrollEmployerAddress.
    /// </summary>
    [JsonPropertyName("scrollEmployerAddress")]
    public EmployerAddress ScrollEmployerAddress
    {
      get => scrollEmployerAddress ??= new();
      set => scrollEmployerAddress = value;
    }

    private Common includeInactive;
    private Employer searchEmployer;
    private EmployerAddress searchEmployerAddress;
    private Employer scrollEmployer;
    private EmployerAddress scrollEmployerAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailEmployer.
      /// </summary>
      [JsonPropertyName("gdetailEmployer")]
      public Employer GdetailEmployer
      {
        get => gdetailEmployer ??= new();
        set => gdetailEmployer = value;
      }

      /// <summary>
      /// A value of GcreateDate.
      /// </summary>
      [JsonPropertyName("gcreateDate")]
      public DateWorkArea GcreateDate
      {
        get => gcreateDate ??= new();
        set => gcreateDate = value;
      }

      /// <summary>
      /// A value of GeiwoEnd.
      /// </summary>
      [JsonPropertyName("geiwoEnd")]
      public DateWorkArea GeiwoEnd
      {
        get => geiwoEnd ??= new();
        set => geiwoEnd = value;
      }

      /// <summary>
      /// A value of Gend.
      /// </summary>
      [JsonPropertyName("gend")]
      public DateWorkArea Gend
      {
        get => gend ??= new();
        set => gend = value;
      }

      /// <summary>
      /// A value of GdetailEmployerAddress.
      /// </summary>
      [JsonPropertyName("gdetailEmployerAddress")]
      public EmployerAddress GdetailEmployerAddress
      {
        get => gdetailEmployerAddress ??= new();
        set => gdetailEmployerAddress = value;
      }

      /// <summary>
      /// A value of GdetailPrompt.
      /// </summary>
      [JsonPropertyName("gdetailPrompt")]
      public Common GdetailPrompt
      {
        get => gdetailPrompt ??= new();
        set => gdetailPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gdetailCommon;
      private Employer gdetailEmployer;
      private DateWorkArea gcreateDate;
      private DateWorkArea geiwoEnd;
      private DateWorkArea gend;
      private EmployerAddress gdetailEmployerAddress;
      private Common gdetailPrompt;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public TextWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of Local30Day.
    /// </summary>
    [JsonPropertyName("local30Day")]
    public DateWorkArea Local30Day
    {
      get => local30Day ??= new();
      set => local30Day = value;
    }

    /// <summary>
    /// A value of CurentDate.
    /// </summary>
    [JsonPropertyName("curentDate")]
    public DateWorkArea CurentDate
    {
      get => curentDate ??= new();
      set => curentDate = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of SearchEmployer.
    /// </summary>
    [JsonPropertyName("searchEmployer")]
    public Employer SearchEmployer
    {
      get => searchEmployer ??= new();
      set => searchEmployer = value;
    }

    /// <summary>
    /// A value of SearchEmployerAddress.
    /// </summary>
    [JsonPropertyName("searchEmployerAddress")]
    public EmployerAddress SearchEmployerAddress
    {
      get => searchEmployerAddress ??= new();
      set => searchEmployerAddress = value;
    }

    private DateWorkAttributes dateWorkAttributes;
    private TextWorkArea textDate;
    private DateWorkArea local30Day;
    private DateWorkArea curentDate;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Employer searchEmployer;
    private EmployerAddress searchEmployerAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private EmployerRelation employerRelation;
    private IncomeSource incomeSource;
    private EmployerAddress employerAddress;
    private Employer employer;
  }
#endregion
}
