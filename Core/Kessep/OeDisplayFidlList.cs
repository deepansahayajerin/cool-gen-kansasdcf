// Program: OE_DISPLAY_FIDL_LIST, ID: 374389517, model: 746.
// Short name: SWE02610
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_DISPLAY_FIDL_LIST.
/// </summary>
[Serializable]
public partial class OeDisplayFidlList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DISPLAY_FIDL_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDisplayFidlList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDisplayFidlList.
  /// </summary>
  public OeDisplayFidlList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------
    // Date             Developer Name         Description
    // 05/01/2000       Sherri Newman-SRS      Source
    // 07/18/2000       George Vandy           PR 98226 - Modifications for FIDM
    // identifier
    //                                         
    // change.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.CsePersonsWorkSet.Number =
      import.FinancialInstitutionDataMatch.CsePersonNumber;
    UseSiReadCsePerson();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadFinancialInstitutionDataMatch2())
      {
        export.Group.Next();
      }
    }
    else
    {
      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadFinancialInstitutionDataMatch1())
    {
      if (Equal(local.Hold.InstitutionTin,
        entities.FinancialInstitutionDataMatch.InstitutionTin) && Equal
        (local.Hold.MatchedPayeeAccountNumber,
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber) && Equal
        (local.Hold.AccountType,
        entities.FinancialInstitutionDataMatch.AccountType))
      {
        export.Group.Next();

        continue;
      }

      local.Hold.Assign(entities.FinancialInstitutionDataMatch);
      export.Group.Update.FinancialInstitutionDataMatch.Assign(
        entities.FinancialInstitutionDataMatch);
      local.Code.CodeName = "FIDM RESOURCE TYPE";
      local.CodeValue.Cdvalue =
        entities.FinancialInstitutionDataMatch.AccountType;
      UseCabValidateCodeValue();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Group.Next();

        break;
      }
      else
      {
        export.Group.Update.AccountType.Text3 =
          Substring(local.CodeValue.Description, 1, 2);
      }

      export.Group.Update.RunDate.Text6 =
        Substring(export.Group.Item.FinancialInstitutionDataMatch.MatchRunDate,
        FinancialInstitutionDataMatch.MatchRunDate_MaxLength, 5, 2) + Substring
        (entities.FinancialInstitutionDataMatch.MatchRunDate,
        FinancialInstitutionDataMatch.MatchRunDate_MaxLength, 1, 4);

      if (AsChar(entities.FinancialInstitutionDataMatch.MsfidmIndicator) == 'Y')
      {
        export.Group.Update.MatchSource.Text6 = "MSFIDM";
      }
      else
      {
        export.Group.Update.MatchSource.Text6 = "FIDM";
      }

      export.Group.Next();
    }

    ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private IEnumerable<bool> ReadFinancialInstitutionDataMatch1()
  {
    return ReadEach("ReadFinancialInstitutionDataMatch1",
      (db, command) =>
      {
        db.SetString(
          command, "cseNumber",
          import.FinancialInstitutionDataMatch.CsePersonNumber);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.FinancialInstitutionDataMatch.CsePersonNumber =
          db.GetString(reader, 0);
        entities.FinancialInstitutionDataMatch.InstitutionTin =
          db.GetString(reader, 1);
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
          db.GetString(reader, 2);
        entities.FinancialInstitutionDataMatch.MatchRunDate =
          db.GetString(reader, 3);
        entities.FinancialInstitutionDataMatch.AccountBalance =
          db.GetNullableDecimal(reader, 4);
        entities.FinancialInstitutionDataMatch.AccountType =
          db.GetString(reader, 5);
        entities.FinancialInstitutionDataMatch.MsfidmIndicator =
          db.GetNullableString(reader, 6);
        entities.FinancialInstitutionDataMatch.InstitutionName =
          db.GetNullableString(reader, 7);
        entities.FinancialInstitutionDataMatch.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFinancialInstitutionDataMatch2()
  {
    return ReadEach("ReadFinancialInstitutionDataMatch2",
      (db, command) =>
      {
        db.SetString(
          command, "cseNumber",
          import.FinancialInstitutionDataMatch.CsePersonNumber);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.FinancialInstitutionDataMatch.CsePersonNumber =
          db.GetString(reader, 0);
        entities.FinancialInstitutionDataMatch.InstitutionTin =
          db.GetString(reader, 1);
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
          db.GetString(reader, 2);
        entities.FinancialInstitutionDataMatch.MatchRunDate =
          db.GetString(reader, 3);
        entities.FinancialInstitutionDataMatch.AccountBalance =
          db.GetNullableDecimal(reader, 4);
        entities.FinancialInstitutionDataMatch.AccountType =
          db.GetString(reader, 5);
        entities.FinancialInstitutionDataMatch.MsfidmIndicator =
          db.GetNullableString(reader, 6);
        entities.FinancialInstitutionDataMatch.InstitutionName =
          db.GetNullableString(reader, 7);
        entities.FinancialInstitutionDataMatch.Populated = true;

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
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of FinancialInstitutionDataMatch.
      /// </summary>
      [JsonPropertyName("financialInstitutionDataMatch")]
      public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
      {
        get => financialInstitutionDataMatch ??= new();
        set => financialInstitutionDataMatch = value;
      }

      /// <summary>
      /// A value of AccountType.
      /// </summary>
      [JsonPropertyName("accountType")]
      public WorkArea AccountType
      {
        get => accountType ??= new();
        set => accountType = value;
      }

      /// <summary>
      /// A value of RunDate.
      /// </summary>
      [JsonPropertyName("runDate")]
      public WorkArea RunDate
      {
        get => runDate ??= new();
        set => runDate = value;
      }

      /// <summary>
      /// A value of MatchSource.
      /// </summary>
      [JsonPropertyName("matchSource")]
      public WorkArea MatchSource
      {
        get => matchSource ??= new();
        set => matchSource = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private FinancialInstitutionDataMatch financialInstitutionDataMatch;
      private WorkArea accountType;
      private WorkArea runDate;
      private WorkArea matchSource;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateCcyy.
    /// </summary>
    [JsonPropertyName("dateCcyy")]
    public TextWorkArea DateCcyy
    {
      get => dateCcyy ??= new();
      set => dateCcyy = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public FinancialInstitutionDataMatch Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    private TextWorkArea dateCcyy;
    private CodeValue codeValue;
    private Code code;
    private FinancialInstitutionDataMatch hold;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }
#endregion
}
