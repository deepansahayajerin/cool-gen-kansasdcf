// Program: SI_REGI_CHECK_PERSON_PROGRAMS, ID: 372816802, model: 746.
// Short name: SWE01268
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_REGI_CHECK_PERSON_PROGRAMS.
/// </summary>
[Serializable]
public partial class SiRegiCheckPersonPrograms: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CHECK_PERSON_PROGRAMS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCheckPersonPrograms(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCheckPersonPrograms.
  /// </summary>
  public SiRegiCheckPersonPrograms(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date    Developer       Request #       Description
    // 07/29/99  M. Lachowicz                    Initial Development
    // 09/13/01  M. Lachowicz    PR 127465       Set discontinue date to
    //                                           
    // the earliest future effective date - 1
    // day.
    // ----------------------------------------------------------
    local.Current.Date = Now().Date;
    local.CsePerson.Number = import.CsePersonsWorkSet.Number;
    UseSiEabRetrieveAdabasPrsnPgms();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      if (!Lt(local.Current.Date, local.Group.Item.Det.ProgEffectiveDate) && !
        Lt(local.Group.Item.Det.ProgramEndDate, local.Current.Date))
      {
        export.ActivePrograms.Flag = "Y";

        break;
      }

      if (Lt(local.Current.Date, local.Group.Item.Det.ProgEffectiveDate))
      {
        if (IsEmpty(export.FuturePrograms.Flag))
        {
          // 09/13/01 M. Lachowicz Start
          export.DiscontinueDate.DiscontinueDate =
            local.Group.Item.Det.ProgEffectiveDate;

          // 09/13/01 M. Lachowicz End
        }
        else
        {
          // 09/13/01 M. Lachowicz Start
          if (Lt(local.Group.Item.Det.ProgEffectiveDate,
            export.DiscontinueDate.DiscontinueDate))
          {
            export.DiscontinueDate.DiscontinueDate =
              local.Group.Item.Det.ProgEffectiveDate;
          }

          // 09/13/01 M. Lachowicz End
        }

        export.FuturePrograms.Flag = "Y";
      }
    }

    local.Group.CheckIndex();

    if (AsChar(export.FuturePrograms.Flag) == 'Y')
    {
      export.DiscontinueDate.DiscontinueDate =
        AddDays(export.DiscontinueDate.DiscontinueDate, -1);
    }
  }

  private static void MoveGroup1(SiEabRetrieveAdabasPrsnPgms.Export.
    GroupGroup source, Local.GroupGroup target)
  {
    target.Det.Assign(source.Det);
  }

  private static void MoveGroup2(Local.GroupGroup source,
    SiEabRetrieveAdabasPrsnPgms.Export.GroupGroup target)
  {
    target.Det.Assign(source.Det);
  }

  private void UseSiEabRetrieveAdabasPrsnPgms()
  {
    var useImport = new SiEabRetrieveAdabasPrsnPgms.Import();
    var useExport = new SiEabRetrieveAdabasPrsnPgms.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Current.Date = local.Current.Date;
    local.Group.CopyTo(useExport.Group, MoveGroup2);

    Call(SiEabRetrieveAdabasPrsnPgms.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Group, MoveGroup1);
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
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public PersonProgram DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
    }

    /// <summary>
    /// A value of FuturePrograms.
    /// </summary>
    [JsonPropertyName("futurePrograms")]
    public Common FuturePrograms
    {
      get => futurePrograms ??= new();
      set => futurePrograms = value;
    }

    /// <summary>
    /// A value of ActivePrograms.
    /// </summary>
    [JsonPropertyName("activePrograms")]
    public Common ActivePrograms
    {
      get => activePrograms ??= new();
      set => activePrograms = value;
    }

    private PersonProgram discontinueDate;
    private Common futurePrograms;
    private Common activePrograms;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      public InterfacePersonProgram Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private InterfacePersonProgram det;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private CsePerson csePerson;
    private DateWorkArea current;
    private Array<GroupGroup> group;
  }
#endregion
}
