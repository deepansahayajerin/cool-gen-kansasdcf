// Program: OE_HICL_HEALTH_INSU_COMPANY_LIST, ID: 371860252, model: 746.
// Short name: SWEHICLP
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
/// A program: OE_HICL_HEALTH_INSU_COMPANY_LIST.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// This procedure(PRAD) allows a user to display a list of all the health 
/// insurance companies, and to select a specific one for return use by other
/// screens.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHiclHealthInsuCompanyList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICL_HEALTH_INSU_COMPANY_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiclHealthInsuCompanyList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiclHealthInsuCompanyList.
  /// </summary>
  public OeHiclHealthInsuCompanyList(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // Rebecca Grimes	01/02/95	Initial Code
    // Sid		01/31/95	Rework/Completion
    // T.O.Redmond	02/15/96	Retrofit
    // R. Marchman	11/14/96	Add new security and next tran.
    // Sid		04/21/97	Change logic to scroll more than the view size.
    // P Phinney       09/27/00        H00104202 - Do NOT return to HIPH if 
    // Carrier = CONV
    // Vithal Madhira  01/19/01        PR# 111764 Health Ins. Co. w/o address  
    // not
    //                                 
    // displaying on screen. Fixed this
    // problem.
    // Mashworth       10/01/01        WR# 20125 Add start and end date to HICO
    //                                 
    // Changed view size from 140 to
    // 110.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      MoveHealthInsuranceCompanyAddress(import.
        StartingHealthInsuranceCompanyAddress,
        export.StartingHealthInsuranceCompanyAddress);

      if (!IsEmpty(export.StartingHealthInsuranceCompanyAddress.State))
      {
        UseOeCabSetMnemonics();
        local.StateCodeValue.Cdvalue =
          export.StartingHealthInsuranceCompanyAddress.State ?? Spaces(10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field =
            GetField(export.StartingHealthInsuranceCompanyAddress, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";

          return;
        }
      }

      export.NextHealthInsuranceCompany.InsurancePolicyCarrier = "";
      export.NextHealthInsuranceCompanyAddress.City = "";
      export.NextHealthInsuranceCompanyAddress.State = "";
    }

    // ---------------------------------------------
    // Move all Import Views to Export Views, Local
    // Views and Export Next Views.
    // ---------------------------------------------
    export.HiddenCase.Number = import.HiddenCase.Number;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.HiddenCsePersonsWorkSet.Assign(import.HiddenCsePersonsWorkSet);
    export.ShowAll.Flag = import.ShowAll.Flag;
    export.StartingHealthInsuranceCompany.InsurancePolicyCarrier =
      import.StartingHealthInsuranceCompany.InsurancePolicyCarrier;
    MoveHealthInsuranceCompanyAddress(import.
      StartingHealthInsuranceCompanyAddress,
      export.StartingHealthInsuranceCompanyAddress);
    export.NextHealthInsuranceCompany.InsurancePolicyCarrier =
      import.NextHealthInsuranceCompany.InsurancePolicyCarrier;
    MoveHealthInsuranceCompanyAddress(import.NextHealthInsuranceCompanyAddress,
      export.NextHealthInsuranceCompanyAddress);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // P Phinney       09/27/00        H00104202 - Do NOT return to HIPH if 
    // Carrier = CONV
    export.HiddenFromHiph.NextTransaction =
      import.HiddenFromHiph.NextTransaction;

    if (IsEmpty(export.ShowAll.Flag))
    {
      export.ShowAll.Flag = "N";
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      // No data is being passed from this procedure.
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
      // *No data is anticipated from next tran for this*
      // *procedure.
      // 
      // *
      // ************************************************
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // ---------------------------------------------
    // Populate the Export Group View prior to any
    // logic.
    // ---------------------------------------------
    if (!import.Import1.IsEmpty)
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

        export.Export1.Update.DetailWork.Selection =
          import.Import1.Item.DetailWork.Selection;
        export.Export1.Update.DetailHealthInsuranceCompany.Assign(
          import.Import1.Item.DetailHealthInsuranceCompany);
        export.Export1.Update.DetailHealthInsuranceCompanyAddress.Assign(
          import.Import1.Item.DetailHealthInsuranceCompanyAddress);
        export.Export1.Update.DetailExpiredFlag.Flag =
          import.Import1.Item.DetailExpiredFlag.Flag;
        export.Export1.Next();
      }
    }

    // ---------------------------------------------
    // Identify the record which was selected.
    // Verify that only one selection was made and
    // the selection character was valid ("S").
    // ---------------------------------------------
    if (!export.Export1.IsEmpty)
    {
      export.HiclSelectionCount.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.DetailWork.Selection))
        {
          case 'S':
            // ---------------------------------------------
            // To select a record, the user must type "S"
            // and press enter. If some other PF Key was
            // pressed, skip this logic.
            // ---------------------------------------------
            if (!Equal(global.Command, "RETURN") && !
              Equal(global.Command, "HICO"))
            {
              goto Test;
            }

            // ---------------------------------------------
            // Move the first selected record into the
            // export_selected views for passing to the
            // called procedure.
            // ---------------------------------------------
            if (IsEmpty(export.SelectedHealthInsuranceCompany.
              InsurancePolicyCarrier))
            {
              export.SelectedHealthInsuranceCompany.Assign(
                export.Export1.Item.DetailHealthInsuranceCompany);
              export.SelectedHealthInsuranceCompanyAddress.Assign(
                export.Export1.Item.DetailHealthInsuranceCompanyAddress);

              if (Equal(global.Command, "RETURN"))
              {
                ExitState = "ACO_NE0000_RETURN";
              }

              if (Equal(global.Command, "HICO"))
              {
                ExitState = "ECO_XFR_TO_HICO";
              }
            }

            // P Phinney       09/27/00        H00104202 - Do NOT return to HIPH
            // if Carrier = CONV
            if (Equal(global.Command, "RETURN"))
            {
              if (Equal(export.HiddenFromHiph.NextTransaction, "HIPH"))
              {
                if (Equal(export.SelectedHealthInsuranceCompany.CarrierCode,
                  "CONV"))
                {
                  var field1 =
                    GetField(export.Export1.Item.DetailWork, "selection");

                  field1.Error = true;

                  ExitState = "NO_RETURN_CARRIER_CONV";

                  goto Test;
                }
              }
            }

            ++export.HiclSelectionCount.Count;

            if (export.HiclSelectionCount.Count > 1)
            {
              // ---------------------------------------------
              // Make selection character ERROR.
              // ---------------------------------------------
              var field1 =
                GetField(export.Export1.Item.DetailWork, "selection");

              field1.Error = true;

              ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
            }

            break;
          case ' ':
            // ---------------------------------------------
            // No action is taken but SPACES is a valid entry.
            // ---------------------------------------------
            break;
          default:
            // ---------------------------------------------
            // Make selection character ERROR.
            // ---------------------------------------------
            var field = GetField(export.Export1.Item.DetailWork, "selection");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "NEXT"))
    {
      if (IsEmpty(export.NextHealthInsuranceCompany.InsurancePolicyCarrier) && IsEmpty
        (export.NextHealthInsuranceCompanyAddress.City) && IsEmpty
        (export.NextHealthInsuranceCompanyAddress.State))
      {
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      }
      else
      {
        // ---------------------------------------------
        // If the Next View is populated, it indicates
        // that the database has more records than the
        // cardinality of the Group View. So do a read
        // again using the Next View as the starting
        // value.
        // ---------------------------------------------
        export.StartingHealthInsuranceCompany.InsurancePolicyCarrier =
          export.NextHealthInsuranceCompany.InsurancePolicyCarrier;
        MoveHealthInsuranceCompanyAddress(export.
          NextHealthInsuranceCompanyAddress,
          export.StartingHealthInsuranceCompanyAddress);
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "RETURN") || Equal(global.Command, "HICO") || Equal
      (global.Command, "ENTER"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      if (AsChar(export.ShowAll.Flag) == 'Y')
      {
        if (ReadProfile())
        {
          if (Equal(entities.Profile.Name, "CO EMPL") || Equal
            (entities.Profile.Name, "DEVELOPERS"))
          {
            local.ShowAll.Flag = "Y";
          }
          else
          {
            local.ShowAll.Flag = "N";
          }
        }
        else
        {
          local.ShowAll.Flag = "N";
        }
      }
      else
      {
        local.ShowAll.Flag = "N";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "HICO":
        if (export.HiclSelectionCount.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_PG_4_CRITERIA";

        break;
      case "RETURN":
        export.HiddenFromHiph.NextTransaction = "";
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SPACES":
        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        // ---------------------------------------------
        // This command will only be executed if the
        // transaction is EXECUTE FIRST on a flow and
        // the COMMAND has not been set in the flow or
        // if the transaction is started from clear
        // screen.
        // ---------------------------------------------
        break;
      case "EXIT":
        // ---------------------------------------------
        // Allows the user to flow back to the next
        // higher menu.  The flow to the menu should be
        // DISPLAY FIRST and command set to <none>.
        // ---------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "DISPLAY":
        UseOeHiclListHealthInsCarrier();

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    global.Command = "";
  }

  private static void MoveExport1(OeHiclListHealthInsCarrier.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailWork.Selection = source.DetailWork.Selection;
    target.DetailHealthInsuranceCompany.Assign(
      source.DetailHealthInsuranceCompany);
    target.DetailHealthInsuranceCompanyAddress.Assign(
      source.DetailHealthInsuranceCompanyAddress);
    target.DetailExpiredFlag.Flag = source.DetailExpiredFlag.Flag;
  }

  private static void MoveHealthInsuranceCompanyAddress(
    HealthInsuranceCompanyAddress source, HealthInsuranceCompanyAddress target)
  {
    target.City = source.City;
    target.State = source.State;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.StateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.StateCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.StateCode.CodeName = useExport.State.CodeName;
  }

  private void UseOeHiclListHealthInsCarrier()
  {
    var useImport = new OeHiclListHealthInsCarrier.Import();
    var useExport = new OeHiclListHealthInsCarrier.Export();

    MoveHealthInsuranceCompanyAddress(export.
      StartingHealthInsuranceCompanyAddress,
      useImport.StartingHealthInsuranceCompanyAddress);
    useImport.StartingHealthInsuranceCompany.InsurancePolicyCarrier =
      export.StartingHealthInsuranceCompany.InsurancePolicyCarrier;
    useImport.ShowAll.Flag = local.ShowAll.Flag;

    Call(OeHiclListHealthInsCarrier.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    MoveHealthInsuranceCompanyAddress(useExport.
      LastHealthInsuranceCompanyAddress,
      export.NextHealthInsuranceCompanyAddress);
    export.NextHealthInsuranceCompany.InsurancePolicyCarrier =
      useExport.LastHealthInsuranceCompany.InsurancePolicyCarrier;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private bool ReadProfile()
  {
    entities.Profile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.Profile.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailWork.
      /// </summary>
      [JsonPropertyName("detailWork")]
      public ScrollingAttributes DetailWork
      {
        get => detailWork ??= new();
        set => detailWork = value;
      }

      /// <summary>
      /// A value of DetailHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("detailHealthInsuranceCompany")]
      public HealthInsuranceCompany DetailHealthInsuranceCompany
      {
        get => detailHealthInsuranceCompany ??= new();
        set => detailHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of DetailHealthInsuranceCompanyAddress.
      /// </summary>
      [JsonPropertyName("detailHealthInsuranceCompanyAddress")]
      public HealthInsuranceCompanyAddress DetailHealthInsuranceCompanyAddress
      {
        get => detailHealthInsuranceCompanyAddress ??= new();
        set => detailHealthInsuranceCompanyAddress = value;
      }

      /// <summary>
      /// A value of DetailExpiredFlag.
      /// </summary>
      [JsonPropertyName("detailExpiredFlag")]
      public Common DetailExpiredFlag
      {
        get => detailExpiredFlag ??= new();
        set => detailExpiredFlag = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private ScrollingAttributes detailWork;
      private HealthInsuranceCompany detailHealthInsuranceCompany;
      private HealthInsuranceCompanyAddress detailHealthInsuranceCompanyAddress;
      private Common detailExpiredFlag;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
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
    /// A value of StartingHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("startingHealthInsuranceCompany")]
    public HealthInsuranceCompany StartingHealthInsuranceCompany
    {
      get => startingHealthInsuranceCompany ??= new();
      set => startingHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of NextHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("nextHealthInsuranceCompany")]
    public HealthInsuranceCompany NextHealthInsuranceCompany
    {
      get => nextHealthInsuranceCompany ??= new();
      set => nextHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of StartingHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("startingHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress StartingHealthInsuranceCompanyAddress
    {
      get => startingHealthInsuranceCompanyAddress ??= new();
      set => startingHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of NextHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("nextHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress NextHealthInsuranceCompanyAddress
    {
      get => nextHealthInsuranceCompanyAddress ??= new();
      set => nextHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedHealthInsuranceCompany
    {
      get => selectedHealthInsuranceCompany ??= new();
      set => selectedHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress SelectedHealthInsuranceCompanyAddress
    {
      get => selectedHealthInsuranceCompanyAddress ??= new();
      set => selectedHealthInsuranceCompanyAddress = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenFromHiph.
    /// </summary>
    [JsonPropertyName("hiddenFromHiph")]
    public Standard HiddenFromHiph
    {
      get => hiddenFromHiph ??= new();
      set => hiddenFromHiph = value;
    }

    private Common showAll;
    private Standard standard;
    private HealthInsuranceCompany startingHealthInsuranceCompany;
    private HealthInsuranceCompany nextHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress startingHealthInsuranceCompanyAddress;
    private HealthInsuranceCompanyAddress nextHealthInsuranceCompanyAddress;
    private HealthInsuranceCompany selectedHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress selectedHealthInsuranceCompanyAddress;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard hiddenFromHiph;
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
      /// A value of DetailWork.
      /// </summary>
      [JsonPropertyName("detailWork")]
      public ScrollingAttributes DetailWork
      {
        get => detailWork ??= new();
        set => detailWork = value;
      }

      /// <summary>
      /// A value of DetailHealthInsuranceCompany.
      /// </summary>
      [JsonPropertyName("detailHealthInsuranceCompany")]
      public HealthInsuranceCompany DetailHealthInsuranceCompany
      {
        get => detailHealthInsuranceCompany ??= new();
        set => detailHealthInsuranceCompany = value;
      }

      /// <summary>
      /// A value of DetailHealthInsuranceCompanyAddress.
      /// </summary>
      [JsonPropertyName("detailHealthInsuranceCompanyAddress")]
      public HealthInsuranceCompanyAddress DetailHealthInsuranceCompanyAddress
      {
        get => detailHealthInsuranceCompanyAddress ??= new();
        set => detailHealthInsuranceCompanyAddress = value;
      }

      /// <summary>
      /// A value of DetailExpiredFlag.
      /// </summary>
      [JsonPropertyName("detailExpiredFlag")]
      public Common DetailExpiredFlag
      {
        get => detailExpiredFlag ??= new();
        set => detailExpiredFlag = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private ScrollingAttributes detailWork;
      private HealthInsuranceCompany detailHealthInsuranceCompany;
      private HealthInsuranceCompanyAddress detailHealthInsuranceCompanyAddress;
      private Common detailExpiredFlag;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of StartingHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("startingHealthInsuranceCompany")]
    public HealthInsuranceCompany StartingHealthInsuranceCompany
    {
      get => startingHealthInsuranceCompany ??= new();
      set => startingHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of NextHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("nextHealthInsuranceCompany")]
    public HealthInsuranceCompany NextHealthInsuranceCompany
    {
      get => nextHealthInsuranceCompany ??= new();
      set => nextHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of StartingHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("startingHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress StartingHealthInsuranceCompanyAddress
    {
      get => startingHealthInsuranceCompanyAddress ??= new();
      set => startingHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of NextHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("nextHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress NextHealthInsuranceCompanyAddress
    {
      get => nextHealthInsuranceCompanyAddress ??= new();
      set => nextHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedHealthInsuranceCompany
    {
      get => selectedHealthInsuranceCompany ??= new();
      set => selectedHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress SelectedHealthInsuranceCompanyAddress
    {
      get => selectedHealthInsuranceCompanyAddress ??= new();
      set => selectedHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HiclSelectionCount.
    /// </summary>
    [JsonPropertyName("hiclSelectionCount")]
    public Common HiclSelectionCount
    {
      get => hiclSelectionCount ??= new();
      set => hiclSelectionCount = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenFromHiph.
    /// </summary>
    [JsonPropertyName("hiddenFromHiph")]
    public Standard HiddenFromHiph
    {
      get => hiddenFromHiph ??= new();
      set => hiddenFromHiph = value;
    }

    private Common showAll;
    private HealthInsuranceCompany startingHealthInsuranceCompany;
    private HealthInsuranceCompany nextHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress startingHealthInsuranceCompanyAddress;
    private HealthInsuranceCompanyAddress nextHealthInsuranceCompanyAddress;
    private HealthInsuranceCompany selectedHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress selectedHealthInsuranceCompanyAddress;
    private Common hiclSelectionCount;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Case1 hiddenCase;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard hiddenFromHiph;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of StateCode.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public Code StateCode
    {
      get => stateCode ??= new();
      set => stateCode = value;
    }

    /// <summary>
    /// A value of StateCodeValue.
    /// </summary>
    [JsonPropertyName("stateCodeValue")]
    public CodeValue StateCodeValue
    {
      get => stateCodeValue ??= new();
      set => stateCodeValue = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public ScrollingAttributes Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public Common Selection
    {
      get => selection ??= new();
      set => selection = value;
    }

    private Common showAll;
    private Common validCode;
    private Code stateCode;
    private CodeValue stateCodeValue;
    private ScrollingAttributes work;
    private Common selection;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    private ServiceProvider serviceProvider;
    private ServiceProviderProfile serviceProviderProfile;
    private Profile profile;
  }
#endregion
}
