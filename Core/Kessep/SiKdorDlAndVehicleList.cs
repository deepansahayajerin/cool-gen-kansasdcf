// Program: SI_KDOR_DL_AND_VEHICLE_LIST, ID: 1625325081, model: 746.
// Short name: SWEKDORP
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
/// A program: SI_KDOR_DL_AND_VEHICLE_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiKdorDlAndVehicleList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_KDOR_DL_AND_VEHICLE_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiKdorDlAndVehicleList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiKdorDlAndVehicleList.
  /// </summary>
  public SiKdorDlAndVehicleList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 12/01/18  GVandy	CQ61419		Initial Code.  Created from a copy of EIWL.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CsePersonsWorkSet.FormattedName =
      import.CsePersonsWorkSet.FormattedName;
    export.CsePerson.Number = import.CsePerson.Number;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    MoveWorkArea(import.RecordType, export.RecordType);
    export.HiddenRecordType.Text3 = import.HiddenRecordType.Text3;
    export.PromptRecordType.SelectChar = import.PromptRecordType.SelectChar;
    export.HiddenCsePersonAddress.Assign(import.HiddenCsePersonAddress);
    export.AdditionalPfKeys.Text35 = import.AdditionalPfKeys.Text35;
    export.DeletePfKey.Text10 = import.DeletePfKey.Text10;
    export.VehicleNumber.Count = import.VehicleNumber.Count;
    export.TotalVehicles.Count = import.TotalVehicles.Count;
    export.KdorVehicle.Identifier = import.KdorVehicle.Identifier;
    export.DeleteEligible.Flag = import.DeleteEligible.Flag;
    export.XofX.Text18 = import.XofX.Text18;

    // -- If any of the search fields are spaces then space out the associated 
    // descriptions.
    if (IsEmpty(export.CsePerson.Number))
    {
      export.CsePersonsWorkSet.FormattedName = "";
    }

    // -- Zero fill any necessary search fields.
    if (!IsEmpty(export.CsePerson.Number))
    {
      UseCabZeroFillNumber();
    }

    // -- Move import group to export group.
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.G.Text80 = import.Import1.Item.G.Text80;
      export.Export1.Update.GexportHighlight.Flag =
        import.Import1.Item.GimportHighlight.Flag;

      if (AsChar(import.Import1.Item.GimportHighlight.Flag) == 'Y')
      {
        var field = GetField(export.Export1.Item.G, "text80");

        field.Color = "yellow";
        field.Protected = true;
      }

      export.Export1.Next();
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETNAME":
        export.PromptPerson.SelectChar = "";

        if (IsEmpty(import.FromName.Number))
        {
        }
        else
        {
          export.CsePerson.Number = import.FromName.Number;
          export.CsePersonsWorkSet.FormattedName =
            import.FromName.FormattedName;
        }

        global.Command = "DISPLAY";

        break;
      case "RETCDVL":
        export.PromptRecordType.SelectChar = "";

        if (IsEmpty(import.FromCdvl.Cdvalue))
        {
        }
        else
        {
          export.RecordType.Text3 = import.FromCdvl.Cdvalue;
          export.RecordType.Text12 = import.FromCdvl.Description;
        }

        global.Command = "DISPLAY";

        break;
      case "RETADDR":
        return;
      case "FROMMENU":
        if (IsEmpty(import.CsePerson.Number))
        {
          // -- Don't change command if a person number was not passed from 
          // PLOM.
        }
        else
        {
          // -- Change command to a display using the person number passed from 
          // PLOM.
          global.Command = "DISPLAY";
        }

        break;
      default:
        // -- Continue
        break;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumber = export.CsePerson.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.CsePerson.Number = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ---------------------------------------------
    // -- Prompt field validation.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST"))
    {
      local.PromptCount.Count = 0;

      switch(AsChar(export.PromptPerson.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptPerson, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.PromptRecordType.SelectChar))
      {
        case 'S':
          ++local.PromptCount.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptRecordType, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      switch(local.PromptCount.Count)
      {
        case 0:
          var field1 = GetField(export.PromptPerson, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptRecordType, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          break;
        case 1:
          break;
        default:
          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            var field3 = GetField(export.PromptPerson, "selectChar");

            field3.Error = true;

            var field4 = GetField(export.PromptRecordType, "selectChar");

            field4.Error = true;
          }
          else
          {
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      switch(AsChar(export.PromptPerson.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptPerson, "selectChar");

          field.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      switch(AsChar(export.PromptRecordType.SelectChar))
      {
        case '+':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.PromptRecordType, "selectChar");

          field.Error = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "NXTMBR":
        if (!Equal(export.AdditionalPfKeys.Text35,
          "19 PrevVehicle  20 NextVehicle"))
        {
          global.Command = "INVALID";

          break;
        }

        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number) || !
          Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
        {
          if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }

          if (!Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
          {
            var field = GetField(export.RecordType, "text3");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (export.VehicleNumber.Count < export.TotalVehicles.Count)
        {
          // -- Increment vehicle number and set command to display.
          ++export.VehicleNumber.Count;
          global.Command = "DISPLAY";

          break;
        }
        else
        {
          ExitState = "SI0000_NO_MORE_VEHICLES";

          return;
        }

        if (export.KdorVehicle.Identifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_ACTION";

          return;
        }

        break;
      case "PRVMBR":
        if (!Equal(export.AdditionalPfKeys.Text35,
          "19 PrevVehicle  20 NextVehicle"))
        {
          global.Command = "INVALID";

          break;
        }

        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number) || !
          Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
        {
          if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }

          if (!Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
          {
            var field = GetField(export.RecordType, "text3");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (export.VehicleNumber.Count == 1)
        {
          ExitState = "SI0000_NO_PREVIOUS_VEHICLES";

          return;
        }
        else if (export.VehicleNumber.Count > 1)
        {
          // -- Decrement vehicle number and set command to display.
          --export.VehicleNumber.Count;
          global.Command = "DISPLAY";

          break;
        }

        if (export.KdorVehicle.Identifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_ACTION";

          return;
        }

        break;
      case "DISPLAY":
        export.KdorVehicle.Identifier = 0;
        export.AdditionalPfKeys.Text35 = "";
        export.DeleteEligible.Flag = "";
        export.DeletePfKey.Text10 = "";
        export.TotalVehicles.Count = 0;
        export.VehicleNumber.Count = 1;
        export.XofX.Text18 = "";

        break;
      case "ADDR":
        if (!Equal(export.AdditionalPfKeys.Text35, "15 ADDR"))
        {
          global.Command = "INVALID";
        }
        else
        {
          if (ReadCase())
          {
            export.ToAddrCase.Number = entities.Case1.Number;
          }

          export.ToAddrCommon.SelectChar = "S";
          export.ToAddrCsePersonsWorkSet.Number = export.CsePerson.Number;
          export.ToAddrCsePersonsWorkSet.FormattedName =
            export.CsePersonsWorkSet.FormattedName;
          ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";

          return;
        }

        break;
      case "DELETE":
        if (!Equal(export.DeletePfKey.Text10, "10 Delete"))
        {
          global.Command = "INVALID";
        }

        break;
      default:
        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "PREV":
        if (export.KdorVehicle.Identifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_ACTION";

          return;
        }

        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        if (export.KdorVehicle.Identifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_ACTION";

          return;
        }

        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "DISPLAY":
        export.XofX.Text18 = "";

        if (IsEmpty(export.RecordType.Text3))
        {
          // --Default record type to KDL
          export.RecordType.Text3 = "KDL";
        }

        local.Code.CodeName = "KDOR TYPE";
        local.CodeValue.Cdvalue = export.RecordType.Text3;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
          export.RecordType.Text12 = local.CodeValue.Description;
        }
        else
        {
          export.RecordType.Text12 = "";
        }

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.RecordType, "text3");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }

        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
        {
          export.CsePersonsWorkSet.FormattedName = "";
        }

        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number) || !
          Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
        {
          export.VehicleNumber.Count = 1;
        }

        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(TrimEnd(export.RecordType.Text3))
        {
          case "KDL":
            UseSiKdorDisplayDriversLicense();

            if (!IsEmpty(export.HiddenCsePersonAddress.Street1))
            {
              export.AdditionalPfKeys.Text35 = "15 ADDR";
            }

            break;
          case "VEH":
            UseSiKdorDisplayVehicleInfo();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.AdditionalPfKeys.Text35 = "19 PrevVehicle  20 NextVehicle";
              export.DeletePfKey.Text10 = "10 Delete";
            }

            break;
          default:
            break;
        }

        if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.GexportHighlight.Flag) == 'Y')
          {
            local.DifferencesFound.Flag = "Y";

            var field = GetField(export.Export1.Item.G, "text80");

            field.Color = "yellow";
            field.Protected = true;
          }
          else
          {
            var field = GetField(export.Export1.Item.G, "text80");

            field.Color = "";
            field.Protected = true;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.HiddenCsePerson.Number = export.CsePerson.Number;
        export.HiddenRecordType.Text3 = export.RecordType.Text3;

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (AsChar(local.DifferencesFound.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_REVIEW_FOR_UPDATE";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "DELETE":
        if (export.KdorVehicle.Identifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number) || !
          Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
        {
          if (!Equal(export.CsePerson.Number, export.HiddenCsePerson.Number))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }

          if (!Equal(export.RecordType.Text3, export.HiddenRecordType.Text3))
          {
            var field = GetField(export.RecordType, "text3");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";
        }

        if (AsChar(export.DeleteEligible.Flag) != 'Y')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field = GetField(export.Export1.Item.G, "text80");

            field.Color = "red";
            field.Protected = true;

            break;
          }

          ExitState = "SI0000_INVALID_VEHICLE_DELETE";

          return;
        }

        UseSiKdorDeleteVehicleInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --Initialize group view.
          export.Export1.Index = 0;
          export.Export1.Clear();

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Next();
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "LIST":
        if (AsChar(export.PromptPerson.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_NAME";
        }
        else
        {
        }

        if (AsChar(export.PromptRecordType.SelectChar) == 'S')
        {
          export.ToCdvl.CodeName = "KDOR TYPE";
          ExitState = "ECO_LNK_TO_CODE_VALUES";
        }
        else
        {
        }

        break;
      case "FROMMENU":
        // -- No processing required.
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveExport2(SiKdorDisplayDriversLicense.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.G.Text80 = source.G.Text80;
    target.GexportHighlight.Flag = source.GexportHighlight.Flag;
  }

  private static void MoveExport3(SiKdorDisplayVehicleInfo.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.G.Text80 = source.G.Text80;
    target.GexportHighlight.Flag = source.GexportHighlight.Flag;
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text3 = source.Text3;
    target.Text12 = source.Text12;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePerson.Number = useImport.CsePerson.Number;
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
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private void UseSiKdorDeleteVehicleInfo()
  {
    var useImport = new SiKdorDeleteVehicleInfo.Import();
    var useExport = new SiKdorDeleteVehicleInfo.Export();

    useImport.KdorVehicle.Identifier = export.KdorVehicle.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiKdorDeleteVehicleInfo.Execute, useImport, useExport);
  }

  private void UseSiKdorDisplayDriversLicense()
  {
    var useImport = new SiKdorDisplayDriversLicense.Import();
    var useExport = new SiKdorDisplayDriversLicense.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiKdorDisplayDriversLicense.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
    export.HiddenCsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiKdorDisplayVehicleInfo()
  {
    var useImport = new SiKdorDisplayVehicleInfo.Import();
    var useExport = new SiKdorDisplayVehicleInfo.Export();

    useImport.VehicleNumber.Count = export.VehicleNumber.Count;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiKdorDisplayVehicleInfo.Execute, useImport, useExport);

    export.DeleteEligible.Flag = useExport.DeleteEligible.Flag;
    export.TotalVehicles.Count = useExport.TotalVehicles.Count;
    export.VehicleNumber.Count = useExport.VehicleNumber.Count;
    export.KdorVehicle.Identifier = useExport.KdorVehicle.Identifier;
    export.XofX.Text18 = useExport.XofX.Text18;
    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimportHighlight.
      /// </summary>
      [JsonPropertyName("gimportHighlight")]
      public Common GimportHighlight
      {
        get => gimportHighlight ??= new();
        set => gimportHighlight = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private WorkArea g;
      private Common gimportHighlight;
    }

    /// <summary>
    /// A value of DeleteEligible.
    /// </summary>
    [JsonPropertyName("deleteEligible")]
    public Common DeleteEligible
    {
      get => deleteEligible ??= new();
      set => deleteEligible = value;
    }

    /// <summary>
    /// A value of DeletePfKey.
    /// </summary>
    [JsonPropertyName("deletePfKey")]
    public WorkArea DeletePfKey
    {
      get => deletePfKey ??= new();
      set => deletePfKey = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonAddress.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonAddress")]
    public CsePersonAddress HiddenCsePersonAddress
    {
      get => hiddenCsePersonAddress ??= new();
      set => hiddenCsePersonAddress = value;
    }

    /// <summary>
    /// A value of AdditionalPfKeys.
    /// </summary>
    [JsonPropertyName("additionalPfKeys")]
    public WorkArea AdditionalPfKeys
    {
      get => additionalPfKeys ??= new();
      set => additionalPfKeys = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
    }

    /// <summary>
    /// A value of VehicleNumber.
    /// </summary>
    [JsonPropertyName("vehicleNumber")]
    public Common VehicleNumber
    {
      get => vehicleNumber ??= new();
      set => vehicleNumber = value;
    }

    /// <summary>
    /// A value of TotalVehicles.
    /// </summary>
    [JsonPropertyName("totalVehicles")]
    public Common TotalVehicles
    {
      get => totalVehicles ??= new();
      set => totalVehicles = value;
    }

    /// <summary>
    /// A value of XofX.
    /// </summary>
    [JsonPropertyName("xofX")]
    public WorkArea XofX
    {
      get => xofX ??= new();
      set => xofX = value;
    }

    /// <summary>
    /// A value of MaxHidden.
    /// </summary>
    [JsonPropertyName("maxHidden")]
    public KdorVehicle MaxHidden
    {
      get => maxHidden ??= new();
      set => maxHidden = value;
    }

    /// <summary>
    /// A value of CurrentHidden.
    /// </summary>
    [JsonPropertyName("currentHidden")]
    public KdorVehicle CurrentHidden
    {
      get => currentHidden ??= new();
      set => currentHidden = value;
    }

    /// <summary>
    /// A value of HiddenRecordType.
    /// </summary>
    [JsonPropertyName("hiddenRecordType")]
    public WorkArea HiddenRecordType
    {
      get => hiddenRecordType ??= new();
      set => hiddenRecordType = value;
    }

    /// <summary>
    /// A value of PromptRecordType.
    /// </summary>
    [JsonPropertyName("promptRecordType")]
    public Common PromptRecordType
    {
      get => promptRecordType ??= new();
      set => promptRecordType = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of FromName.
    /// </summary>
    [JsonPropertyName("fromName")]
    public CsePersonsWorkSet FromName
    {
      get => fromName ??= new();
      set => fromName = value;
    }

    private Common deleteEligible;
    private WorkArea deletePfKey;
    private CsePersonAddress hiddenCsePersonAddress;
    private WorkArea additionalPfKeys;
    private KdorVehicle kdorVehicle;
    private Common vehicleNumber;
    private Common totalVehicles;
    private WorkArea xofX;
    private KdorVehicle maxHidden;
    private KdorVehicle currentHidden;
    private WorkArea hiddenRecordType;
    private Common promptRecordType;
    private WorkArea recordType;
    private CodeValue fromCdvl;
    private Array<ImportGroup> import1;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common promptPerson;
    private NextTranInfo hiddenNextTranInfo;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet fromName;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportHighlight.
      /// </summary>
      [JsonPropertyName("gexportHighlight")]
      public Common GexportHighlight
      {
        get => gexportHighlight ??= new();
        set => gexportHighlight = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private WorkArea g;
      private Common gexportHighlight;
    }

    /// <summary>
    /// A value of ToAddrCommon.
    /// </summary>
    [JsonPropertyName("toAddrCommon")]
    public Common ToAddrCommon
    {
      get => toAddrCommon ??= new();
      set => toAddrCommon = value;
    }

    /// <summary>
    /// A value of ToAddrCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("toAddrCsePersonsWorkSet")]
    public CsePersonsWorkSet ToAddrCsePersonsWorkSet
    {
      get => toAddrCsePersonsWorkSet ??= new();
      set => toAddrCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ToAddrCase.
    /// </summary>
    [JsonPropertyName("toAddrCase")]
    public Case1 ToAddrCase
    {
      get => toAddrCase ??= new();
      set => toAddrCase = value;
    }

    /// <summary>
    /// A value of DeleteEligible.
    /// </summary>
    [JsonPropertyName("deleteEligible")]
    public Common DeleteEligible
    {
      get => deleteEligible ??= new();
      set => deleteEligible = value;
    }

    /// <summary>
    /// A value of DeletePfKey.
    /// </summary>
    [JsonPropertyName("deletePfKey")]
    public WorkArea DeletePfKey
    {
      get => deletePfKey ??= new();
      set => deletePfKey = value;
    }

    /// <summary>
    /// A value of AdditionalPfKeys.
    /// </summary>
    [JsonPropertyName("additionalPfKeys")]
    public WorkArea AdditionalPfKeys
    {
      get => additionalPfKeys ??= new();
      set => additionalPfKeys = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
    }

    /// <summary>
    /// A value of VehicleNumber.
    /// </summary>
    [JsonPropertyName("vehicleNumber")]
    public Common VehicleNumber
    {
      get => vehicleNumber ??= new();
      set => vehicleNumber = value;
    }

    /// <summary>
    /// A value of TotalVehicles.
    /// </summary>
    [JsonPropertyName("totalVehicles")]
    public Common TotalVehicles
    {
      get => totalVehicles ??= new();
      set => totalVehicles = value;
    }

    /// <summary>
    /// A value of XofX.
    /// </summary>
    [JsonPropertyName("xofX")]
    public WorkArea XofX
    {
      get => xofX ??= new();
      set => xofX = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonAddress.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonAddress")]
    public CsePersonAddress HiddenCsePersonAddress
    {
      get => hiddenCsePersonAddress ??= new();
      set => hiddenCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ToCdvl.
    /// </summary>
    [JsonPropertyName("toCdvl")]
    public Code ToCdvl
    {
      get => toCdvl ??= new();
      set => toCdvl = value;
    }

    /// <summary>
    /// A value of HiddenRecordType.
    /// </summary>
    [JsonPropertyName("hiddenRecordType")]
    public WorkArea HiddenRecordType
    {
      get => hiddenRecordType ??= new();
      set => hiddenRecordType = value;
    }

    /// <summary>
    /// A value of PromptRecordType.
    /// </summary>
    [JsonPropertyName("promptRecordType")]
    public Common PromptRecordType
    {
      get => promptRecordType ??= new();
      set => promptRecordType = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    private Common toAddrCommon;
    private CsePersonsWorkSet toAddrCsePersonsWorkSet;
    private Case1 toAddrCase;
    private Common deleteEligible;
    private WorkArea deletePfKey;
    private WorkArea additionalPfKeys;
    private KdorVehicle kdorVehicle;
    private Common vehicleNumber;
    private Common totalVehicles;
    private WorkArea xofX;
    private CsePersonAddress hiddenCsePersonAddress;
    private Code toCdvl;
    private WorkArea hiddenRecordType;
    private Common promptRecordType;
    private WorkArea recordType;
    private Array<ExportGroup> export1;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common promptPerson;
    private NextTranInfo hiddenNextTranInfo;
    private CsePerson hiddenCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalHighlight.
      /// </summary>
      [JsonPropertyName("glocalHighlight")]
      public Common GlocalHighlight
      {
        get => glocalHighlight ??= new();
        set => glocalHighlight = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private WorkArea g;
      private Common glocalHighlight;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of DifferencesFound.
    /// </summary>
    [JsonPropertyName("differencesFound")]
    public Common DifferencesFound
    {
      get => differencesFound ??= new();
      set => differencesFound = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Array<LocalGroup> local1;
    private Common differencesFound;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common promptCount;
    private DateWorkArea null1;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
