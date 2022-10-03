// Program: SI_PEPR_READ_PERSON_PROGRAMS, ID: 371736653, model: 746.
// Short name: SWE01233
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
/// A program: SI_PEPR_READ_PERSON_PROGRAMS.
/// </para>
/// <para>
/// This AB reads all the programs that a person is currently on.
/// </para>
/// </summary>
[Serializable]
public partial class SiPeprReadPersonPrograms: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_READ_PERSON_PROGRAMS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprReadPersonPrograms(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprReadPersonPrograms.
  /// </summary>
  public SiPeprReadPersonPrograms(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 09/15/95  Helen Sharland                Initial Development
    // 06/03/97  Sid	                        Cleanup and Fixes.
    // 10/20/98  C Deghand                     Modified the statement
    //                                         
    // displaying the MORE
    //                                         
    // indicator to show it all
    //                                         
    // the time.
    // 11/12/98  C Deghand                     Modified the READ EACH to
    //                                         
    // display the open programs
    //                                         
    // on top.
    // ----------------------------------------------------------
    // 02/26/99 W.Campbell         Deleted statement move
    //                             import standard to export standard
    //                             from this CAB, as it was not doing
    //                             anything, and gave a consistency
    //                             check warning message as such.
    // ------------------------------------------------------------
    // ***************************************************************
    // 10/13/99   C. Ott   PR # 73777.  Moved statement that sets scrolling 
    // message.  Program was being escaped prior to execution
    // **************************************************************
    // ***************************************************************
    // 11/12/99   C. Ott   PR # 80196.  Modified READ REACH statement to be more
    // fully qualified in order to correct scrolling errors.
    // **************************************************************
    UseCabReadAdabasPerson();

    if (!IsEmpty(export.AbendData.Type1))
    {
      return;
    }

    export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    // -------------------------------------------------------------
    // 02/26/99 W.Campbell - Deleted statement
    // move import standard to export standard
    // from this CAB, as it was not doing anything,
    // and gave a warning message as such.
    // ------------------------------------------------------------
    MovePersonProgram(import.PagePersonProgram, local.PagePersonProgram);
    local.PageProgram.Code = import.PageProgram.Code;
    UseSiFormatCsePersonName();
    export.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    UseCabSetMaximumDiscontinueDate();

    if (Equal(import.PagePersonProgram.DiscontinueDate, local.Blank.Date))
    {
      local.PagePersonProgram.DiscontinueDate = local.Max.Date;
    }

    if (Equal(import.PagePersonProgram.EffectiveDate, local.Blank.Date))
    {
      local.PagePersonProgram.EffectiveDate = local.Max.Date;
    }

    export.Export1.Index = 0;
    export.Export1.CheckSize();

    foreach(var item in ReadPersonProgramProgram())
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (import.Standard.PageNumber == 1)
      {
        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          export.Standard.ScrollingMessage = "MORE +";
        }
        else
        {
          export.Standard.ScrollingMessage = "MORE";
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

      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.PagePersonProgram.EffectiveDate =
          entities.PersonProgram.EffectiveDate;
        export.PagePersonProgram.DiscontinueDate =
          entities.PersonProgram.DiscontinueDate;
        export.PageProgram.Code = entities.Program.Code;

        return;
      }

      export.Export1.Update.DetailProgram.Assign(entities.Program);
      export.Export1.Update.DetailPersonProgram.Assign(entities.PersonProgram);

      // ***	If assigned date exists set to created date, else derive from 
      // created timestamp.
      if (Lt(local.Blank.Date, entities.PersonProgram.AssignedDate))
      {
        export.Export1.Update.DetailCreated.Date =
          entities.PersonProgram.AssignedDate;
      }
      else
      {
        export.Export1.Update.DetailCreated.Date =
          Date(entities.PersonProgram.CreatedTimestamp);
      }

      if (Equal(entities.PersonProgram.DiscontinueDate, local.Max.Date))
      {
        export.Export1.Update.DetailPersonProgram.DiscontinueDate = null;
      }

      if (Equal(entities.PersonProgram.MedTypeDiscontinueDate, local.Max.Date))
      {
        export.Export1.Update.DetailPersonProgram.MedTypeDiscontinueDate = null;
      }
    }
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.PagePersonProgram.DiscontinueDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          local.PagePersonProgram.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "code", local.PageProgram.Code);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 14);
        entities.Program.Code = db.GetString(reader, 15);
        entities.Program.Title = db.GetString(reader, 16);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of PagePersonProgram.
    /// </summary>
    [JsonPropertyName("pagePersonProgram")]
    public PersonProgram PagePersonProgram
    {
      get => pagePersonProgram ??= new();
      set => pagePersonProgram = value;
    }

    /// <summary>
    /// A value of PageProgram.
    /// </summary>
    [JsonPropertyName("pageProgram")]
    public Program PageProgram
    {
      get => pageProgram ??= new();
      set => pageProgram = value;
    }

    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PersonProgram pagePersonProgram;
    private Program pageProgram;
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
      /// A value of DetailProgram.
      /// </summary>
      [JsonPropertyName("detailProgram")]
      public Program DetailProgram
      {
        get => detailProgram ??= new();
        set => detailProgram = value;
      }

      /// <summary>
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
      }

      /// <summary>
      /// A value of DetailPersonProgram.
      /// </summary>
      [JsonPropertyName("detailPersonProgram")]
      public PersonProgram DetailPersonProgram
      {
        get => detailPersonProgram ??= new();
        set => detailPersonProgram = value;
      }

      /// <summary>
      /// A value of Prompt2.
      /// </summary>
      [JsonPropertyName("prompt2")]
      public Common Prompt2
      {
        get => prompt2 ??= new();
        set => prompt2 = value;
      }

      /// <summary>
      /// A value of DetailCreated.
      /// </summary>
      [JsonPropertyName("detailCreated")]
      public DateWorkArea DetailCreated
      {
        get => detailCreated ??= new();
        set => detailCreated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCommon;
      private Program detailProgram;
      private Common prompt;
      private PersonProgram detailPersonProgram;
      private Common prompt2;
      private DateWorkArea detailCreated;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of PagePersonProgram.
    /// </summary>
    [JsonPropertyName("pagePersonProgram")]
    public PersonProgram PagePersonProgram
    {
      get => pagePersonProgram ??= new();
      set => pagePersonProgram = value;
    }

    /// <summary>
    /// A value of PageProgram.
    /// </summary>
    [JsonPropertyName("pageProgram")]
    public Program PageProgram
    {
      get => pageProgram ??= new();
      set => pageProgram = value;
    }

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
    private Standard standard;
    private PersonProgram pagePersonProgram;
    private Program pageProgram;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PageProgram.
    /// </summary>
    [JsonPropertyName("pageProgram")]
    public Program PageProgram
    {
      get => pageProgram ??= new();
      set => pageProgram = value;
    }

    /// <summary>
    /// A value of PagePersonProgram.
    /// </summary>
    [JsonPropertyName("pagePersonProgram")]
    public PersonProgram PagePersonProgram
    {
      get => pagePersonProgram ??= new();
      set => pagePersonProgram = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private Program pageProgram;
    private PersonProgram pagePersonProgram;
    private DateWorkArea blank;
    private DateWorkArea max;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    private CsePerson csePerson;
    private Program program;
    private PersonProgram personProgram;
  }
#endregion
}
