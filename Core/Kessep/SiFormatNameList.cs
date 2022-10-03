// Program: SI_FORMAT_NAME_LIST, ID: 371455759, model: 746.
// Short name: SWE01165
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_FORMAT_NAME_LIST.
/// </summary>
[Serializable]
public partial class SiFormatNameList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FORMAT_NAME_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFormatNameList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFormatNameList.
  /// </summary>
  public SiFormatNameList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------
    // 12/18/98 W.Campbell      Changed logic in an
    //                          IF statement to accomodate
    //                          a new SYSTEM_1 FLAG
    //                          value = 'Z' which represents
    //                          a Non CASE CSE related person.
    // ------------------------------------------------
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      // ------------------------------------------------------------
      // Format name and determine which systems a CSE Person is
      // used by.
      // ------------------------------------------------------------
      MoveCsePersonsWorkSet(import.Import1.Item.Detail,
        export.Export1.Update.DetailCsePersonsWorkSet);

      if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Ssn, "000000000"))
      {
        export.Export1.Update.DetailCsePersonsWorkSet.Ssn = "";
      }

      // ------------------------------------------------------------
      // Suppress the DOB for zero or 01010001 date.
      // ------------------------------------------------------------
      if (!Lt(local.Blank.Date, export.Export1.Item.DetailCsePersonsWorkSet.Dob) ||
        Equal
        (export.Export1.Item.DetailCsePersonsWorkSet.Dob, new DateTime(1, 1, 1)))
        
      {
        export.Export1.Update.DetailCsePersonsWorkSet.Dob = null;
      }

      export.Export1.Update.DetailAlt.Flag = import.Import1.Item.Alt.Flag;

      // ------------------------------------------------------------
      // Set the Select_Char to the Flag value to remove permitted value 
      // violation.
      // ------------------------------------------------------------
      export.Export1.Update.DetailAlt.SelectChar =
        export.Export1.Item.DetailAlt.Flag;

      // ------------------------------------------------------------
      // Interpret systems that the CSE Person is found upon:
      // System 1:
      // A - AE only
      // B - AE and CSE
      // C - CSE only
      // N - Neither AE nor CSE
      // Z - Non Case CSE
      // System 2:
      // K - KANPAY only
      // S - KSCARES only
      // X - KANPAY and KSCARES
      // ------------------------------------------------------------
      // --------------------------------------------
      // Group Import CSE Flag -
      // Y -> Person involved in CSE CASE
      // N -> Non CASE related CSE person
      // Blank -> No CSE involvment type of person
      // --------------------------------------------
      // ------------------------------------------------
      // 12/18/98 W.Campbell - Changed the following
      // IF statement to accomodate the new SYSTEM_1
      // FLAG value = 'Z' which represents a Non
      // CASE CSE related person.
      // ------------------------------------------------
      if (AsChar(import.Import1.Item.Ae.Flag) == 'Y')
      {
        if (AsChar(import.Import1.Item.Cse.Flag) == 'Y' || AsChar
          (import.Import1.Item.Cse.Flag) == 'A')
        {
          export.Export1.Update.DetailSystem1.Text1 = "B";
        }
        else
        {
          export.Export1.Update.DetailSystem1.Text1 = "A";
        }
      }
      else if (AsChar(import.Import1.Item.Cse.Flag) == 'Y' || AsChar
        (import.Import1.Item.Cse.Flag) == 'A')
      {
        export.Export1.Update.DetailSystem1.Text1 = "C";
      }
      else if (AsChar(import.Import1.Item.Cse.Flag) == 'N')
      {
        export.Export1.Update.DetailSystem1.Text1 = "Z";
      }
      else
      {
        export.Export1.Update.DetailSystem1.Text1 = "N";
      }

      if (AsChar(import.Import1.Item.Kanpay.Flag) == 'Y')
      {
        if (AsChar(import.Import1.Item.Kscares.Flag) == 'Y')
        {
          export.Export1.Update.DetailSystem2.Text1 = "X";
        }
        else
        {
          export.Export1.Update.DetailSystem2.Text1 = "K";
        }
      }
      else if (AsChar(import.Import1.Item.Kscares.Flag) == 'Y')
      {
        export.Export1.Update.DetailSystem2.Text1 = "S";
      }
      else
      {
        export.Export1.Update.DetailSystem2.Text1 = "";
      }

      UseSiFormatCsePersonName();
      export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
      export.Export1.Next();
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(import.Import1.Item.Detail);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
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

      /// <summary>
      /// A value of Alt.
      /// </summary>
      [JsonPropertyName("alt")]
      public Common Alt
      {
        get => alt ??= new();
        set => alt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
      private Common alt;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private Array<ImportGroup> import1;
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
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
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
      /// A value of DetailSystem.
      /// </summary>
      [JsonPropertyName("detailSystem")]
      public WorkArea DetailSystem
      {
        get => detailSystem ??= new();
        set => detailSystem = value;
      }

      /// <summary>
      /// A value of DetailSystem1.
      /// </summary>
      [JsonPropertyName("detailSystem1")]
      public WorkArea DetailSystem1
      {
        get => detailSystem1 ??= new();
        set => detailSystem1 = value;
      }

      /// <summary>
      /// A value of DetailSystem2.
      /// </summary>
      [JsonPropertyName("detailSystem2")]
      public WorkArea DetailSystem2
      {
        get => detailSystem2 ??= new();
        set => detailSystem2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Common detailAlt;
      private WorkArea detailSystem;
      private WorkArea detailSystem1;
      private WorkArea detailSystem2;
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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private SsnWorkArea ssnWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea blank;
  }
#endregion
}
