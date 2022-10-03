// Program: OE_SCRUB_SUFFIXES, ID: 371365686, model: 746.
// Short name: SWE03601
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_SCRUB_SUFFIXES.
/// </summary>
[Serializable]
public partial class OeScrubSuffixes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_SCRUB_SUFFIXES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeScrubSuffixes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeScrubSuffixes.
  /// </summary>
  public OeScrubSuffixes(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.CsePersonsWorkSet.LastName = import.CsePersonsWorkSet.LastName;
    local.Space.Flag = "";

    // we will now try to scrape off any suffix the last name might have.
    local.Start.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;

    do
    {
      local.Postion.Text1 =
        Substring(local.CsePersonsWorkSet.LastName, local.CurrentPosition.Count,
        1);

      if (IsEmpty(local.Postion.Text1))
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text17 = "";

        switch(local.FieldNumber.Count)
        {
          case 1:
            if (local.Current.Count == 1)
            {
              local.FindSuffix1.LastName = "";
            }
            else
            {
              local.WorkArea.Text17 =
                Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                local.Current.Count - 1);
              local.FindSuffix1.LastName = local.WorkArea.Text17;
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            if (!IsEmpty(local.FindSuffix1.LastName))
            {
              local.Test.Cdvalue = local.FindSuffix1.LastName;
            }

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.NumberOfDays.Count = 0;
            }
            else
            {
              local.WorkArea.Text17 =
                Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                local.Current.Count - 1);
              local.FindSuffix2.LastName = local.WorkArea.Text17;
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            if (!IsEmpty(local.FindSuffix2.LastName))
            {
              local.Test.Cdvalue = local.FindSuffix2.LastName;
            }

            break;
          case 3:
            if (local.Current.Count == 1)
            {
              local.Suffix3.Cdvalue = local.WorkArea.Text17;
            }
            else
            {
              local.WorkArea.Text17 =
                Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                local.Current.Count - 1);
              local.Suffix3.Cdvalue = local.WorkArea.Text17;
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            if (!IsEmpty(local.Suffix3.Cdvalue))
            {
              local.Test.Cdvalue = local.Suffix3.Cdvalue;
            }

            break;
          case 4:
            if (local.Current.Count == 1)
            {
              local.Suffix4.Cdvalue = local.WorkArea.Text17;
            }
            else
            {
              local.WorkArea.Text17 =
                Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                local.Current.Count - 1);
              local.Suffix4.Cdvalue = local.WorkArea.Text17;
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            if (!IsEmpty(local.Suffix4.Cdvalue))
            {
              local.Test.Cdvalue = local.Suffix4.Cdvalue;
            }

            break;
          case 5:
            if (local.Current.Count == 1)
            {
              local.Suffix5.Cdvalue = local.WorkArea.Text17;
            }
            else
            {
              local.Suffix5.Cdvalue =
                Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            if (!IsEmpty(local.Suffix5.Cdvalue))
            {
              local.Test.Cdvalue = local.Suffix5.Cdvalue;
            }

            break;
          default:
            break;
        }
      }

      if (local.CurrentPosition.Count >= 17)
      {
        if (!IsEmpty(local.Postion.Text1))
        {
          switch(local.FieldNumber.Count)
          {
            case 1:
              if (local.Current.Count == 1)
              {
                local.FindSuffix1.LastName = "";
              }
              else
              {
                local.WorkArea.Text17 =
                  Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                  local.Current.Count);
                local.FindSuffix1.LastName = local.WorkArea.Text17;
              }

              if (!IsEmpty(local.FindSuffix1.LastName))
              {
                local.Test.Cdvalue = local.FindSuffix1.LastName;
              }

              break;
            case 2:
              if (local.Current.Count == 1)
              {
                local.NumberOfDays.Count = 0;
              }
              else
              {
                local.WorkArea.Text17 =
                  Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                  local.Current.Count);
                local.FindSuffix2.LastName = local.WorkArea.Text17;
              }

              if (!IsEmpty(local.FindSuffix2.LastName))
              {
                local.Test.Cdvalue = local.FindSuffix2.LastName;
              }

              break;
            case 3:
              if (local.Current.Count == 1)
              {
                local.Suffix3.Cdvalue = local.WorkArea.Text17;
              }
              else
              {
                local.WorkArea.Text17 =
                  Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                  local.Current.Count);
                local.Suffix3.Cdvalue = local.WorkArea.Text17;
              }

              if (!IsEmpty(local.Suffix3.Cdvalue))
              {
                local.Test.Cdvalue = local.Suffix3.Cdvalue;
              }

              break;
            case 4:
              if (local.Current.Count == 1)
              {
                local.Suffix4.Cdvalue = local.WorkArea.Text17;
              }
              else
              {
                local.WorkArea.Text17 =
                  Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                  local.Current.Count);
                local.Suffix4.Cdvalue = local.WorkArea.Text17;
              }

              if (!IsEmpty(local.Suffix4.Cdvalue))
              {
                local.Test.Cdvalue = local.Suffix4.Cdvalue;
              }

              break;
            case 5:
              if (local.Current.Count == 1)
              {
                local.Suffix5.Cdvalue = local.WorkArea.Text17;
              }
              else
              {
                local.WorkArea.Text17 =
                  Substring(local.CsePersonsWorkSet.LastName, local.Start.Count,
                  local.Current.Count);
              }

              if (!IsEmpty(local.Suffix5.Cdvalue))
              {
                local.Test.Cdvalue = local.Suffix5.Cdvalue;
              }

              break;
            default:
              break;
          }
        }

        break;
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    local.SuffixFound.Flag = "";

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      if (Equal(local.Test.Cdvalue, import.Group.Item.CodeValue.Cdvalue))
      {
        local.SuffixFound.Flag = "Y";

        break;
      }
    }

    import.Group.CheckIndex();

    if (AsChar(local.SuffixFound.Flag) == 'Y')
    {
      // put the last name back togather but do not include the suffix
      local.CsePersonsWorkSet.LastName = "";

      if (Equal(local.Test.Cdvalue, local.Suffix5.Cdvalue))
      {
        local.CsePersonsWorkSet.LastName =
          TrimEnd(local.FindSuffix1.LastName) + local.Space.Flag + TrimEnd
          (local.FindSuffix2.LastName) + local.Space.Flag + TrimEnd
          (local.Suffix3.Cdvalue) + local.Space.Flag + TrimEnd
          (local.Suffix4.Cdvalue);
      }
      else if (Equal(local.Test.Cdvalue, local.Suffix4.Cdvalue))
      {
        local.CsePersonsWorkSet.LastName =
          TrimEnd(local.FindSuffix1.LastName) + local.Space.Flag + TrimEnd
          (local.FindSuffix2.LastName) + local.Space.Flag + TrimEnd
          (local.Suffix3.Cdvalue);
      }
      else if (Equal(local.Test.Cdvalue, local.Suffix3.Cdvalue))
      {
        local.CsePersonsWorkSet.LastName =
          TrimEnd(local.FindSuffix1.LastName) + local.Space.Flag + TrimEnd
          (local.FindSuffix2.LastName);
      }
      else if (Equal(local.Test.Cdvalue, local.FindSuffix2.LastName))
      {
        local.CsePersonsWorkSet.LastName = local.FindSuffix1.LastName;
      }
    }
    else
    {
      // the last name did not have any suffixes so it is ok to process as it is
    }

    export.CsePersonsWorkSet.LastName = local.CsePersonsWorkSet.LastName;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of CodeValue.
      /// </summary>
      [JsonPropertyName("codeValue")]
      public CodeValue CodeValue
      {
        get => codeValue ??= new();
        set => codeValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private CodeValue codeValue;
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

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Array<GroupGroup> group;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of FindSuffix1.
    /// </summary>
    [JsonPropertyName("findSuffix1")]
    public CsePersonsWorkSet FindSuffix1
    {
      get => findSuffix1 ??= new();
      set => findSuffix1 = value;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public CodeValue Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of FindSuffix2.
    /// </summary>
    [JsonPropertyName("findSuffix2")]
    public CsePersonsWorkSet FindSuffix2
    {
      get => findSuffix2 ??= new();
      set => findSuffix2 = value;
    }

    /// <summary>
    /// A value of Suffix3.
    /// </summary>
    [JsonPropertyName("suffix3")]
    public CodeValue Suffix3
    {
      get => suffix3 ??= new();
      set => suffix3 = value;
    }

    /// <summary>
    /// A value of Suffix4.
    /// </summary>
    [JsonPropertyName("suffix4")]
    public CodeValue Suffix4
    {
      get => suffix4 ??= new();
      set => suffix4 = value;
    }

    /// <summary>
    /// A value of Suffix5.
    /// </summary>
    [JsonPropertyName("suffix5")]
    public CodeValue Suffix5
    {
      get => suffix5 ??= new();
      set => suffix5 = value;
    }

    /// <summary>
    /// A value of SuffixFound.
    /// </summary>
    [JsonPropertyName("suffixFound")]
    public Common SuffixFound
    {
      get => suffixFound ??= new();
      set => suffixFound = value;
    }

    /// <summary>
    /// A value of Space.
    /// </summary>
    [JsonPropertyName("space")]
    public Common Space
    {
      get => space ??= new();
      set => space = value;
    }

    private Common start;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private TextWorkArea postion;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea workArea;
    private CsePersonsWorkSet findSuffix1;
    private CodeValue test;
    private Common numberOfDays;
    private CsePersonsWorkSet findSuffix2;
    private CodeValue suffix3;
    private CodeValue suffix4;
    private CodeValue suffix5;
    private Common suffixFound;
    private Common space;
  }
#endregion
}
