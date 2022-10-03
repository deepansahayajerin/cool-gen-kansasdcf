// Program: OE_MILI_LIST_MILITARY_SERVICE, ID: 371920132, model: 746.
// Short name: SWE00948
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
/// A program: OE_MILI_LIST_MILITARY_SERVICE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeMiliListMilitaryService: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MILI_LIST_MILITARY_SERVICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMiliListMilitaryService(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMiliListMilitaryService.
  /// </summary>
  public OeMiliListMilitaryService(IContext context, Import import,
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
    // ---------------------------------------------
    // Date    	Developer	Description
    // 00/00/00	Sid		Initial Code
    // 01/11/96  	T.O.Redmond	Add Monthly Pay and BAQ Allotment
    // 				from Income Source and add Country/State
    // 				prompts
    // 02/06/96	T.O.Redmond	Retrofit Changes
    // ---------------------------------------------
    export.Group.Number = import.CsePerson.Number;
    UseOeCabCheckCaseMember();

    if (!IsEmpty(local.ExportWork.Flag))
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (AsChar(import.FromIncs.Flag) != 'Y')
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadMilitaryService2())
      {
        export.Export1.Update.PageDetail.EffectiveDate =
          entities.ExistingMilitaryService.EffectiveDate;
        export.Export1.Next();
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "MILITARY_SERVICE_NF";
      }
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadMilitaryService1())
      {
        export.Export1.Update.PageDetail.EffectiveDate =
          entities.ExistingMilitaryService.EffectiveDate;
        export.Export1.Next();
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "MILITARY_NF_FOR_INCS";
      }
    }
  }

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.ExportWork.Flag = useExport.Work.Flag;
    export.Name.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadMilitaryService1()
  {
    return ReadEach("ReadMilitaryService1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingMilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.ExistingMilitaryService.CspNumber = db.GetString(reader, 1);
        entities.ExistingMilitaryService.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMilitaryService2()
  {
    return ReadEach("ReadMilitaryService2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingMilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.ExistingMilitaryService.CspNumber = db.GetString(reader, 1);
        entities.ExistingMilitaryService.Populated = true;

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
    /// A value of FromIncs.
    /// </summary>
    [JsonPropertyName("fromIncs")]
    public Common FromIncs
    {
      get => fromIncs ??= new();
      set => fromIncs = value;
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

    private Common fromIncs;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
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
      /// A value of PageDetail.
      /// </summary>
      [JsonPropertyName("pageDetail")]
      public MilitaryService PageDetail
      {
        get => pageDetail ??= new();
        set => pageDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private MilitaryService pageDetail;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
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
    /// A value of Group.
    /// </summary>
    [JsonPropertyName("group")]
    public CsePerson Group
    {
      get => group ??= new();
      set => group = value;
    }

    private CsePersonsWorkSet name;
    private Array<ExportGroup> export1;
    private CsePerson group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExportWork.
    /// </summary>
    [JsonPropertyName("exportWork")]
    public Common ExportWork
    {
      get => exportWork ??= new();
      set => exportWork = value;
    }

    private Common exportWork;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("existingPersonIncomeHistory")]
    public PersonIncomeHistory ExistingPersonIncomeHistory
    {
      get => existingPersonIncomeHistory ??= new();
      set => existingPersonIncomeHistory = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingMilitaryService.
    /// </summary>
    [JsonPropertyName("existingMilitaryService")]
    public MilitaryService ExistingMilitaryService
    {
      get => existingMilitaryService ??= new();
      set => existingMilitaryService = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private PersonIncomeHistory existingPersonIncomeHistory;
    private IncomeSource existingIncomeSource;
    private MilitaryService existingMilitaryService;
    private CsePerson existingCsePerson;
  }
#endregion
}
