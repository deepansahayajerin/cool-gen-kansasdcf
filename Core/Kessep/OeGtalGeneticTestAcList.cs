// Program: OE_GTAL_GENETIC_TEST_AC_LIST, ID: 371792878, model: 746.
// Short name: SWEGTALP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTAL_GENETIC_TEST_AC_LIST.
/// </para>
/// <para>
/// RESP: OBLGEST
/// This procedure displays Genetic Test Account and it's contact person name 
/// and CSE Office.
/// Listing can be from given starting CSE Office or Contact Person's last name.
/// Partial Name can be entered minimum of one alpha character
/// of name.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeGtalGeneticTestAcList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTAL_GENETIC_TEST_AC_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtalGeneticTestAcList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtalGeneticTestAcList.
  /// </summary>
  public OeGtalGeneticTestAcList(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // unknown   	MM/DD/YY	Initial Code
    // T.O.Redmond	02/08/96	Retrofit
    // R. Marchman	11/14/96	Add new security and next tran.
    // A.Kinney	04/30/97	Changed Current_Date
    // ******** END MAINTENANCE LOG ****************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Office.Name = import.Office.Name;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Sel.OneChar = import.Group.Item.Sel.OneChar;
      export.Group.Update.DetGeneticTestAccount.AccountNumber =
        import.Group.Item.DetGeneticTestAccount.AccountNumber;
      export.Group.Update.DetCsePersonsWorkSet.FormattedName =
        import.Group.Item.DetCsePersonsWorkSet.FormattedName;
      export.Group.Update.DetOffice.Name = import.Group.Item.DetOffice.Name;
      export.Group.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *If the next tran info is not equal to spaces, *
      // *this implies the user requested a next tran   *
      // *action. Now validate that action.             *
      // ************************************************
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ************************************************
      // *Example: If not equal to spaces or zeroes     *
      // *Set export cse person number to export next   *
      // *tran hidden info.                             *
      // ************************************************
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        if (!IsEmpty(import.ServiceProvider.LastName))
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadOfficeOfficeServiceProviderServiceProvider1())
          {
            export.Group.Update.DetGeneticTestAccount.AccountNumber =
              entities.ExistingGeneticTestAccount.AccountNumber;
            export.Group.Update.DetOffice.Name = entities.ExistingOffice.Name;
            local.CsePersonsWorkSet.FirstName =
              entities.ExistingServiceProvider.FirstName;
            local.CsePersonsWorkSet.MiddleInitial =
              entities.ExistingServiceProvider.MiddleInitial;
            local.CsePersonsWorkSet.LastName =
              entities.ExistingServiceProvider.LastName;
            UseSiFormatCsePersonName();
            export.Group.Next();
          }
        }
        else
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadOfficeOfficeServiceProviderServiceProvider2())
          {
            export.Group.Update.DetGeneticTestAccount.AccountNumber =
              entities.ExistingGeneticTestAccount.AccountNumber;
            export.Group.Update.DetOffice.Name = entities.ExistingOffice.Name;
            local.CsePersonsWorkSet.FirstName =
              entities.ExistingServiceProvider.FirstName;
            local.CsePersonsWorkSet.MiddleInitial =
              entities.ExistingServiceProvider.MiddleInitial;
            local.CsePersonsWorkSet.LastName =
              entities.ExistingServiceProvider.LastName;
            UseSiFormatCsePersonName();
            export.Group.Next();
          }
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "PREV":
        ExitState = "OE0067_SCROLLED_BEY_FIRST_PAGE";

        break;
      case "NEXT":
        ExitState = "OE0068_SCROLLED_BEY_LAST_PAGE";

        break;
      case "RETURN":
        local.Common.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Sel.OneChar))
          {
            local.Common.Count = (int)((long)local.Common.Count + 1);

            if (AsChar(export.Group.Item.Sel.OneChar) == 'S')
            {
              export.Selected.AccountNumber =
                export.Group.Item.DetGeneticTestAccount.AccountNumber;
            }
            else
            {
              var field = GetField(export.Group.Item.Sel, "oneChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
        }

        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!IsEmpty(export.Group.Item.Sel.OneChar))
            {
              var field = GetField(export.Group.Item.Sel, "oneChar");

              field.Error = true;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK") && local.Common.Count < 2)
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Group.Update.DetCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private IEnumerable<bool> ReadOfficeOfficeServiceProviderServiceProvider1()
  {
    return ReadEach("ReadOfficeOfficeServiceProviderServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "name", import.Office.Name);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "lastName", import.ServiceProvider.LastName);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingGeneticTestAccount.OffGeneratedId =
          db.GetNullableInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingOffice.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingGeneticTestAccount.SpdGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingGeneticTestAccount.OspRoleCode =
          db.GetNullableString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingGeneticTestAccount.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 9);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 10);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 11);
        entities.ExistingGeneticTestAccount.AccountNumber =
          db.GetString(reader, 12);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingGeneticTestAccount.Populated = true;
        entities.ExistingOffice.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeServiceProviderServiceProvider2()
  {
    return ReadEach("ReadOfficeOfficeServiceProviderServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "name", import.Office.Name);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "lastName", import.ServiceProvider.LastName);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingGeneticTestAccount.OffGeneratedId =
          db.GetNullableInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingOffice.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingGeneticTestAccount.SpdGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingGeneticTestAccount.OspRoleCode =
          db.GetNullableString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingGeneticTestAccount.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 9);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 10);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 11);
        entities.ExistingGeneticTestAccount.AccountNumber =
          db.GetString(reader, 12);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingGeneticTestAccount.Populated = true;
        entities.ExistingOffice.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detCsePersonsWorkSet")]
      public CsePersonsWorkSet DetCsePersonsWorkSet
      {
        get => detCsePersonsWorkSet ??= new();
        set => detCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Standard Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of DetOffice.
      /// </summary>
      [JsonPropertyName("detOffice")]
      public Office DetOffice
      {
        get => detOffice ??= new();
        set => detOffice = value;
      }

      /// <summary>
      /// A value of DetGeneticTestAccount.
      /// </summary>
      [JsonPropertyName("detGeneticTestAccount")]
      public GeneticTestAccount DetGeneticTestAccount
      {
        get => detGeneticTestAccount ??= new();
        set => detGeneticTestAccount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private CsePersonsWorkSet detCsePersonsWorkSet;
      private Standard sel;
      private Office detOffice;
      private GeneticTestAccount detGeneticTestAccount;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Standard Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of DetCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detCsePersonsWorkSet")]
      public CsePersonsWorkSet DetCsePersonsWorkSet
      {
        get => detCsePersonsWorkSet ??= new();
        set => detCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetOffice.
      /// </summary>
      [JsonPropertyName("detOffice")]
      public Office DetOffice
      {
        get => detOffice ??= new();
        set => detOffice = value;
      }

      /// <summary>
      /// A value of DetGeneticTestAccount.
      /// </summary>
      [JsonPropertyName("detGeneticTestAccount")]
      public GeneticTestAccount DetGeneticTestAccount
      {
        get => detGeneticTestAccount ??= new();
        set => detGeneticTestAccount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private Standard sel;
      private CsePersonsWorkSet detCsePersonsWorkSet;
      private Office detOffice;
      private GeneticTestAccount detGeneticTestAccount;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public GeneticTestAccount Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private GeneticTestAccount selected;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
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
    /// A value of OfficeFound.
    /// </summary>
    [JsonPropertyName("officeFound")]
    public Common OfficeFound
    {
      get => officeFound ??= new();
      set => officeFound = value;
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
    private Common officeFound;
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
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingGeneticTestAccount.
    /// </summary>
    [JsonPropertyName("existingGeneticTestAccount")]
    public GeneticTestAccount ExistingGeneticTestAccount
    {
      get => existingGeneticTestAccount ??= new();
      set => existingGeneticTestAccount = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private GeneticTestAccount existingGeneticTestAccount;
    private Office existingOffice;
  }
#endregion
}
