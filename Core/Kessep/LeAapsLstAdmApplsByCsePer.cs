// Program: LE_AAPS_LST_ADM_APPLS_BY_CSE_PER, ID: 372580989, model: 746.
// Short name: SWEAAPSP
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
/// A program: LE_AAPS_LST_ADM_APPLS_BY_CSE_PER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAapsLstAdmApplsByCsePer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AAPS_LST_ADM_APPLS_BY_CSE_PER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAapsLstAdmApplsByCsePer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAapsLstAdmApplsByCsePer.
  /// </summary>
  public LeAapsLstAdmApplsByCsePer(IContext context, Import import,
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
    // Every initial development and change to that development needs to be 
    // documented.
    // ---------------------------------------------
    // *******************************************************************
    // Date  	  Developer      Request #  
    // Description
    // 
    // 07/07/95  S. Benton
    // Initial development
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    export.Starting.ReceivedDate = import.Starting.ReceivedDate;
    local.NoOfRecsSelected.Count = 0;

    if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // Dont move to exports since the exports will be populated afresh.
      // ---------------------------------------------
    }
    else if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.AdministrativeAction.Type1 =
          import.Import1.Item.AdministrativeAction.Type1;
        export.Export1.Update.AdministrativeAppeal.Assign(
          import.Import1.Item.AdministrativeAppeal);
        export.Export1.Update.DateWorkArea.Date =
          import.Import1.Item.DateWorkArea.Date;
        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;

        if (!IsEmpty(import.Import1.Item.Common.SelectChar))
        {
          ++local.NoOfRecsSelected.Count;
          export.Selected.Assign(export.Export1.Item.AdministrativeAppeal);

          if (local.NoOfRecsSelected.Count > 1)
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.Next();
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (local.NoOfRecsSelected.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
          (export.CsePersonsWorkSet.Ssn))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

          return;
        }

        MoveCsePersonsWorkSet1(export.CsePersonsWorkSet, local.Saved);

        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);

            if (!IsEmpty(export.CsePersonsWorkSet.Number))
            {
              export.CsePersonsWorkSet.Ssn = "";
              export.SsnWorkArea.SsnNumPart1 = 0;
              export.SsnWorkArea.SsnNumPart2 = 0;
              export.SsnWorkArea.SsnNumPart3 = 0;

              var field = GetField(export.CsePersonsWorkSet, "number");

              field.Error = true;
            }
            else
            {
              var field = GetField(export.CsePersonsWorkSet, "ssn");

              field.Error = true;
            }

            return;
          }
        }
        else if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          local.Search.Flag = "1";
          UseCabMatchCsePerson();

          for(local.CsePersonFromSearch.Index = 0; local
            .CsePersonFromSearch.Index < local.CsePersonFromSearch.Count; ++
            local.CsePersonFromSearch.Index)
          {
            ++local.ReturnFromSsn.Count;
          }

          if (local.ReturnFromSsn.Count == 0)
          {
            MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);

            var field = GetField(export.CsePersonsWorkSet, "ssn");

            field.Error = true;

            ExitState = "LE0000_CSE_PERSON_NF_FOR_SSN";

            return;
          }

          if (local.ReturnFromSsn.Count > 1)
          {
            MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);

            var field = GetField(export.CsePersonsWorkSet, "ssn");

            field.Error = true;

            ExitState = "LE0000_MULTIPLE_PERSONS_FOR_SSN";

            return;
          }

          for(local.CsePersonFromSearch.Index = 0; local
            .CsePersonFromSearch.Index < local.CsePersonFromSearch.Count; ++
            local.CsePersonFromSearch.Index)
          {
            MoveCsePersonsWorkSet2(local.CsePersonFromSearch.Item.Detail,
              export.CsePersonsWorkSet);

            break;
          }

          UseSiFormatCsePersonName();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }
        else
        {
          ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        // *********************************************
        // Insert USE statement here to read all
        // Administrative Appeals for the CSE Person.
        // *********************************************
        UseLeDispAdminAppealsByCsePer();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Export1.IsEmpty)
          {
            ExitState = "LE0000_NO_ADMIN_APPLS_4_PRSN";
          }
          else if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;
        }

        break;
      case "AAPP":
        ExitState = "ECO_LNK_TO_ADMIN_APPEAL";

        break;
      case "HELP":
        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1(LeDispAdminAppealsByCsePer.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DateWorkArea.Date = source.DateWorkArea.Date;
    target.Common.SelectChar = source.Common.SelectChar;
    target.AdministrativeAppeal.Assign(source.AdministrativeAppeal);
    target.AdministrativeAction.Type1 = source.AdministrativeAction.Type1;
  }

  private static void MoveExport1ToCsePersonFromSearch(CabMatchCsePerson.Export.
    ExportGroup source, Local.CsePersonFromSearchGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.DetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    useImport.Search.Flag = local.Search.Flag;
    MoveCsePersonsWorkSet4(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(
      local.CsePersonFromSearch, MoveExport1ToCsePersonFromSearch);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseLeDispAdminAppealsByCsePer()
  {
    var useImport = new LeDispAdminAppealsByCsePer.Import();
    var useExport = new LeDispAdminAppealsByCsePer.Export();

    useImport.Starting.ReceivedDate = export.Starting.ReceivedDate;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(LeDispAdminAppealsByCsePer.Execute, useImport, useExport);

    MoveCsePersonsWorkSet3(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
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
      /// A value of AdministrativeAppeal.
      /// </summary>
      [JsonPropertyName("administrativeAppeal")]
      public AdministrativeAppeal AdministrativeAppeal
      {
        get => administrativeAppeal ??= new();
        set => administrativeAppeal = value;
      }

      /// <summary>
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private DateWorkArea dateWorkArea;
      private Common common;
      private AdministrativeAppeal administrativeAppeal;
      private AdministrativeAction administrativeAction;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public AdministrativeAppeal Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private SsnWorkArea ssnWorkArea;
    private AdministrativeAppeal starting;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Standard nexttran;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
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
      /// A value of AdministrativeAppeal.
      /// </summary>
      [JsonPropertyName("administrativeAppeal")]
      public AdministrativeAppeal AdministrativeAppeal
      {
        get => administrativeAppeal ??= new();
        set => administrativeAppeal = value;
      }

      /// <summary>
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private DateWorkArea dateWorkArea;
      private Common common;
      private AdministrativeAppeal administrativeAppeal;
      private AdministrativeAction administrativeAction;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public AdministrativeAppeal Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public AdministrativeAppeal Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private SsnWorkArea ssnWorkArea;
    private AdministrativeAppeal starting;
    private AdministrativeAppeal selected;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
    private Standard standard;
    private Standard nexttran;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CsePersonFromSearchGroup group.</summary>
    [Serializable]
    public class CsePersonFromSearchGroup
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
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detail;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePersonsWorkSet Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of NoOfRecsSelected.
    /// </summary>
    [JsonPropertyName("noOfRecsSelected")]
    public Common NoOfRecsSelected
    {
      get => noOfRecsSelected ??= new();
      set => noOfRecsSelected = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// Gets a value of CsePersonFromSearch.
    /// </summary>
    [JsonIgnore]
    public Array<CsePersonFromSearchGroup> CsePersonFromSearch =>
      csePersonFromSearch ??= new(CsePersonFromSearchGroup.Capacity);

    /// <summary>
    /// Gets a value of CsePersonFromSearch for json serialization.
    /// </summary>
    [JsonPropertyName("csePersonFromSearch")]
    [Computed]
    public IList<CsePersonFromSearchGroup> CsePersonFromSearch_Json
    {
      get => csePersonFromSearch;
      set => CsePersonFromSearch.Assign(value);
    }

    /// <summary>
    /// A value of ReturnFromSsn.
    /// </summary>
    [JsonPropertyName("returnFromSsn")]
    public Common ReturnFromSsn
    {
      get => returnFromSsn ??= new();
      set => returnFromSsn = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet saved;
    private Common noOfRecsSelected;
    private Common search;
    private Array<CsePersonFromSearchGroup> csePersonFromSearch;
    private Common returnFromSsn;
    private NextTranInfo nextTranInfo;
  }
#endregion
}
